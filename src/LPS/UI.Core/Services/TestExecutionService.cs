using AutoMapper;
using FluentValidation;
using LPS.Domain;
using LPS.Domain.Common;
using LPS.Domain.Domain.Common.Exceptions;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Domain.LPSFlow.LPSHandlers;
using LPS.DTOs;
using LPS.Infrastructure.LPSClients.GlobalVariableManager;
using LPS.Infrastructure.LPSClients.SessionManager;
using LPS.Infrastructure.Nodes;
using LPS.UI.Common.Options;
using LPS.UI.Core.LPSValidators;
using Microsoft.Extensions.Options;
using LPS.Domain.Common.Interfaces;
using LPS.UI.Core.LPSCommandLine;
using LPS.AutoMapper;
using LPS.UI.Core.Host;
using LPS.UI.Common;

namespace LPS.UI.Core.Services
{
    public class TestExecutionService : ITestExecutionService
    {
        private readonly ILogger _logger;
        private readonly IClusterConfiguration _clusterConfiguration;
        private readonly INodeRegistry _nodeRegistry;
        private readonly IEntityDiscoveryService _entityDiscoveryService;
        private readonly IMapper _mapper;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        private readonly IPlaceholderResolverService _placeholderResolverService;
        private readonly IVariableManager _variableManager;
        private readonly IMetricsDataMonitor _lpsMonitoringEnroller;
        private readonly IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> _httpClientManager;
        private readonly IClientConfiguration<HttpRequest> _config;
        private readonly IWatchdog _watchdog;
        private readonly ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationExecutionCommandStatusMonitor;
        private readonly IDashboardService _dashboardService;
        private readonly CancellationTokenSource _cts;

        public TestExecutionService(
            ILogger logger,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            IClusterConfiguration clusterConfiguration,
            INodeRegistry nodeRegistry,
            IEntityDiscoveryService entityDiscoveryService,
            IPlaceholderResolverService placeholderResolverService,
            IVariableManager variableManager,
            IMetricsDataMonitor lpsMonitoringEnroller,
            IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> httpClientManager,
            IClientConfiguration<HttpRequest> config,
            IWatchdog watchdog,
            IDashboardService dashboardService,
            ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> httpIterationExecutionCommandStatusMonitor,
            CancellationTokenSource cts)
        {
            _logger = logger;
            _clusterConfiguration = clusterConfiguration;
            _nodeRegistry = nodeRegistry;
            _entityDiscoveryService = entityDiscoveryService;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _placeholderResolverService = placeholderResolverService;
            _variableManager = variableManager;
            _lpsMonitoringEnroller = lpsMonitoringEnroller;
            _httpClientManager = httpClientManager;
            _config = config;
            _watchdog = watchdog;
            _httpIterationExecutionCommandStatusMonitor = httpIterationExecutionCommandStatusMonitor;
            _dashboardService = dashboardService;
            _cts = cts;
            // Create the AutoMapper configuration
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DtoToCommandProfile(placeholderResolverService, string.Empty));
            });

            // Validate the configuration
            mapperConfig.AssertConfigurationIsValid();

            // Create the mapper instance
            _mapper = mapperConfig.CreateMapper();
        }

        public async Task ExecuteAsync(TestRunParameters parameters)
        {
            var localNode = _nodeRegistry.GetLocalNode();
            var planDto = parameters.IsInline ? parameters.PlanDto : ConfigurationService.FetchConfiguration<PlanDto>(parameters.ConfigFile, _placeholderResolverService);
            new PlanValidator(planDto).ValidateAndThrow(planDto);
            var planCommand = _mapper.Map<Plan.SetupCommand>(planDto);
            var plan = new Plan(planCommand, _logger, _runtimeOperationIdProvider, _placeholderResolverService);
            if (plan.IsValid)
            {
                var variableValidator = new VariableValidator();
                var environmentValidator = new EnvironmentValidator();

                // Global Variables
                foreach (var variableDto in planDto.Variables)
                {
                    variableValidator.ValidateAndThrow(variableDto);
                    var variableHolder = await BuildVariableHolder(variableDto, true, parameters.CancellationToken);
                    _variableManager.AddVariableAsync(variableDto.Name, variableHolder, parameters.CancellationToken).Wait();
                }

                // Environment-Specific Variables
                foreach (var environmentName in parameters.Environments)
                {
                    var environmentDto = planDto.Environments
                        .FirstOrDefault(env => env.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase));

                    if (environmentDto != null)
                    {
                        environmentValidator.ValidateAndThrow(environmentDto);

                        foreach (var variableDto in environmentDto.Variables)
                        {
                            variableValidator.ValidateAndThrow(variableDto);
                            var variableHolder = await BuildVariableHolder(variableDto, false, parameters.CancellationToken);

                            _variableManager.AddVariableAsync(variableDto.Name, variableHolder, parameters.CancellationToken).Wait();
                        }
                    }
                    else
                    {
                        _logger.Log(_runtimeOperationIdProvider.OperationId, $"Environment '{environmentName}' not found.", LPSLoggingLevel.Warning);
                    }
                }

                // Rounds and Iterations
                foreach (var roundDto in planDto.Rounds.Where(round =>
                    parameters.RoundNames.Count == 0 && parameters.Tags.Count == 0 ||
                    parameters.RoundNames.Contains(round.Name, StringComparer.OrdinalIgnoreCase) ||
                    round.Tags.Any(tag => parameters.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))))
                {
                    var roundCommand = _mapper.Map<Round.SetupCommand>(roundDto);
                    var roundEntity = new Round(roundCommand, _logger, _lpsMonitoringEnroller, _runtimeOperationIdProvider);

                    if (roundEntity.IsValid)
                    {
                        foreach (var iterationDto in roundDto.Iterations)
                        {
                            var iterationEntity = ProcessIteration(roundDto, iterationDto);
                            if (iterationEntity.IsValid)
                            {
                                roundEntity.AddIteration(iterationEntity);
                            }
                        }


                        foreach (var referencedIteration in roundDto.ReferencedIterations)
                        {
                            // Find the referenced iteration by name in the global iterations list
                            var globalIteration = planDto.Iterations.FirstOrDefault(i => i.Name.Equals(referencedIteration, StringComparison.OrdinalIgnoreCase));
                            if (globalIteration != null)
                            {
                                var iterationEntity = ProcessIteration(roundDto, globalIteration);
                                if (iterationEntity.IsValid)
                                {
                                    roundEntity.AddIteration(iterationEntity);
                                }
                            }
                            else
                            {
                                _logger.Log(_runtimeOperationIdProvider.OperationId, $"Referenced iteration '{referencedIteration}' not found.", LPSLoggingLevel.Warning);
                            }
                        }
                        plan.AddRound(roundEntity);
                    }
                }
            }

            if (plan.GetReadOnlyRounds().Any())
            {
                RegisterEntities(plan);
                await localNode.SetNodeStatus(NodeStatus.Running);
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Plan '{plan?.Name}' execution has started", LPSLoggingLevel.Information);
                _dashboardService.Start();
                await new Plan.ExecuteCommand(_logger, _watchdog, _runtimeOperationIdProvider, _httpClientManager, _config, _httpIterationExecutionCommandStatusMonitor, _lpsMonitoringEnroller, _cts)
                    .ExecuteAsync(plan);
                await _dashboardService.WaitForDashboardRefreshAsync();
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Plan '{plan?.Name}' execution has completed", LPSLoggingLevel.Information);
            }
            else
            {
                _logger.Log(_runtimeOperationIdProvider.OperationId, "No rounds to execute", LPSLoggingLevel.Information);
            }
        }


        private async Task<VariableHolder> BuildVariableHolder(VariableDto variableDto, bool isGlobal, CancellationToken cancellationToken)
        {
            var mimeType = MimeTypeExtensions.FromKeyword(variableDto.As);
            var builder = new VariableHolder.Builder(_placeholderResolverService);
            return await builder
                .WithFormat(mimeType)
                .WithPattern(variableDto.Regex)
                .WithRawValue(variableDto.Value)
                .SetGlobal(isGlobal)
                .BuildAsync(cancellationToken);
        }

        private HttpIteration ProcessIteration(RoundDto roundDto, HttpIterationDto iterationDto)
        {
            if (iterationDto.HttpRequest?.URL != null && roundDto?.BaseUrl != null && !iterationDto.HttpRequest.URL.StartsWith("http://") && !iterationDto.HttpRequest.URL.StartsWith("https://"))
            {
                if (iterationDto.HttpRequest.URL.StartsWith("$") && roundDto.BaseUrl.StartsWith("$"))
                {
                    throw new InvalidOperationException("Either the base URL or the local URL is defined as a variable, but runtime handling of both as variables is not supported. Consider setting the base URL as a global variable and reusing it in the local variable.");
                }
                iterationDto.HttpRequest.URL = $"{roundDto.BaseUrl}{iterationDto.HttpRequest.URL}";
            }
            var iterationCommand = _mapper.Map<HttpIteration.SetupCommand>(iterationDto);
            var iterationEntity = new HttpIteration(iterationCommand, _logger, _runtimeOperationIdProvider);
            if (iterationEntity.IsValid)
            {
                var requestCommand = _mapper.Map<HttpRequest.SetupCommand>(iterationDto.HttpRequest);
                var request = new HttpRequest(requestCommand, _logger, _runtimeOperationIdProvider);
                if (request.IsValid)
                {
                    if (iterationDto?.HttpRequest?.Capture != null)
                    {
                        var captureCommand = _mapper.Map<CaptureHandler.SetupCommand>(iterationDto.HttpRequest.Capture);
                        var capture = new CaptureHandler(captureCommand, _logger, _runtimeOperationIdProvider);
                        if (capture.IsValid)
                        {
                            request.SetCapture(capture);
                        }
                        else
                        {
                            throw new InvalidLPSEntityException($"Invalid Capture handler defined in the iteration {iterationDto.Name}, Please fix the validation errors and try again");
                        }
                    }
                    iterationEntity.SetHttpRequest(request);
                }
                else
                {
                    throw new InvalidLPSEntityException($"Invalid HttpRequest in the iteration {iterationDto.Name}, Please fix the validation errors and try again");
                }
                return iterationEntity;
            }
            throw new InvalidLPSEntityException($"Invalid Iteration {iterationDto.Name}, Please fix the validation errors and try again");
        }


        public void RegisterEntities(Plan plan)
        {
            var entityRegisterer = new EntityRegisterer(_clusterConfiguration, 
                _nodeRegistry.GetLocalNode().Metadata,
                _entityDiscoveryService, 
                _nodeRegistry);
            entityRegisterer.RegisterEntities(plan);
        }
    }
}
