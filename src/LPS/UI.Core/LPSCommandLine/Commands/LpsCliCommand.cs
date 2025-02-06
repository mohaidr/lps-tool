using LPS.Domain.Common.Interfaces;
using LPS.Domain;
using LPS.UI.Common;
using LPS.UI.Common.Options;
using LPS.UI.Core.LPSCommandLine.Bindings;
using FluentValidation.Results;
using LPS.UI.Common.Extensions;
using LPS.UI.Core.LPSValidators;
using Microsoft.Extensions.Options;
using FluentValidation;
using LPS.UI.Core.Services;
using LPS.DTOs;
using AutoMapper;
using LPS.Domain.Domain.Common.Interfaces;
using System.CommandLine;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class LpsCliCommand : ICliCommand
    {
        private readonly string[] _args;
        private readonly ILogger _logger;
        private readonly IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> _httpClientManager;
        private readonly IClientConfiguration<HttpRequest> _config;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        private readonly IWatchdog _watchdog;
        private readonly Command _rootCliCommand;
        public Command Command => _rootCliCommand;
        private readonly IMetricsDataMonitor _lpsMonitoringEnroller;
        private readonly IPlaceholderResolverService _placeholdersResolverService;
        private readonly ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationExecutionCommandStatusMonitor;
        private readonly CancellationTokenSource _cts;
        private readonly IOptions<DashboardConfigurationOptions> _dashboardConfig;
        private readonly IMapper _mapper; // AutoMapper instance

        public LpsCliCommand(
            Command rootCliCommand,
            ILogger logger,
            IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> httpClientManager,
            IClientConfiguration<HttpRequest> config,
            IWatchdog watchdog,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> httpIterationExecutionCommandStatusMonitor,
            IMetricsDataMonitor lpsMonitoringEnroller,
            IPlaceholderResolverService placeholdersResolverService,
            IOptions<DashboardConfigurationOptions> dashboardConfig,
            IMapper mapper,
            CancellationTokenSource cts,
            string[] args) // Inject AutoMapper
        {
            _rootCliCommand = rootCliCommand;
            _logger = logger;
            _args = args;
            _config = config;
            _httpClientManager = httpClientManager;
            _watchdog = watchdog;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _httpIterationExecutionCommandStatusMonitor = httpIterationExecutionCommandStatusMonitor;
            _lpsMonitoringEnroller = lpsMonitoringEnroller;
            _dashboardConfig = dashboardConfig;
            _placeholdersResolverService = placeholdersResolverService;
            _cts = cts;
            _mapper = mapper; // Assign mapper
            Setup();
        }

        private void Setup()
        {
            CommandLineOptions.AddOptionsToCommand(_rootCliCommand, typeof(CommandLineOptions.LPSCommandOptions));
        }

        public void SetHandler(CancellationToken cancellationToken)
        {
            _rootCliCommand.SetHandler(async (PlanDto planDto, bool save) =>
            {
                try
                {
                    // Validation Results
                    ValidationResult planValidationResults, roundValidationResults, iterationValidationResults, requestValidationResults;

                    // Validate Plan
                    var planValidator = new PlanValidator(planDto);
                    planValidationResults = planValidator.Validate();

                    // Validate Round
                    var roundDto = planDto.Rounds[0];
                    var roundValidator = new RoundValidator(roundDto);
                    roundValidationResults = roundValidator.Validate();

                    // Validate Iteration
                    var iterationDto = roundDto.Iterations[0];
                    var iterationValidator = new IterationValidator(iterationDto);
                    iterationValidationResults = iterationValidator.Validate();

                    // Validate Request
                    var requestValidator = new RequestValidator(iterationDto.HttpRequest);
                    requestValidationResults = requestValidator.Validate();

                    // Proceed if all validations pass
                    if (planValidationResults.IsValid && roundValidationResults.IsValid && iterationValidationResults.IsValid && requestValidationResults.IsValid)
                    {
                        // Map DTOs to Domain Models using AutoMapper
                        var plan = new Plan(_mapper.Map<Plan.SetupCommand>(planDto), _logger, _runtimeOperationIdProvider, _placeholdersResolverService);
                        var testRound = new Round(_mapper.Map<Round.SetupCommand>(roundDto), _logger, _runtimeOperationIdProvider);
                        var httpIteration = new HttpIteration(_mapper.Map<HttpIteration.SetupCommand>(iterationDto), _logger, _runtimeOperationIdProvider);
                        var request =  new HttpRequest(_mapper.Map<HttpRequest.SetupCommand>(iterationDto.HttpRequest), _logger, _runtimeOperationIdProvider);

                        // Establish Relationships
                        httpIteration.SetHttpRequest(request);
                        testRound.AddIteration(httpIteration);
                        plan.AddRound(testRound);

                        // Create and Run Manager
                        var manager = new LPSManager(
                            _logger,
                            _httpClientManager,
                            _config,
                            _watchdog,
                            _runtimeOperationIdProvider,
                            _httpIterationExecutionCommandStatusMonitor,
                            _lpsMonitoringEnroller,
                            _dashboardConfig,
                            _cts);

                        if (save)
                        {
                            ConfigurationService.SaveConfiguration($"{planDto.Name}.yaml", planDto);
                            ConfigurationService.SaveConfiguration($"{planDto.Name}.json", planDto);
                        }

                        await manager.RunAsync(plan);
                    }
                    else
                    {
                        // Print Validation Errors
                        planValidationResults.PrintValidationErrors();
                        roundValidationResults.PrintValidationErrors();
                        iterationValidationResults.PrintValidationErrors();
                        requestValidationResults.PrintValidationErrors();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(
                        _runtimeOperationIdProvider.OperationId,
                        $"{ex.Message}\r\n{ex.InnerException?.Message}\r\n{ex.StackTrace}",
                        LPSLoggingLevel.Error);
                }
            },
            new CommandBinder(),
            CommandLineOptions.LPSCommandOptions.SaveOption);
        }
    }
}
