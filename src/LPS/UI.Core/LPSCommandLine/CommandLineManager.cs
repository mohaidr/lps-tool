using LPS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.CommandLine;
using System.IO;
using Newtonsoft.Json;
using System.CommandLine.Binding;
using LPS.Domain.Common.Interfaces;
using LPS.UI.Common;
using System.Threading;
using LPS.UI.Common.Options;
using Microsoft.Extensions.Options;
using LPS.UI.Core.LPSCommandLine.Commands;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Infrastructure.LPSClients.GlobalVariableManager;
using LPS.Infrastructure.LPSClients.PlaceHolderService;
using AutoMapper;
using LPS.AutoMapper;
using LPS.Infrastructure.Nodes;
using Apis.Common;
using LPS.Common.Interfaces;
using LPS.UI.Core.Services;

namespace LPS.UI.Core.LPSCommandLine
{
    public class CommandLineManager
    {
        private string[] _command_args;
        readonly ILogger _logger;
        IClusterConfiguration _clusterConfiguration;
        ITestOrchestratorService _testOrchestratorService;
        readonly IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> _httpClientManager;
        readonly IClientConfiguration<HttpRequest> _config;
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        readonly IWatchdog _watchdog;
        readonly IVariableManager _variableManager;
        Command _rootCliCommand;
        LpsCliCommand _lpsCliCommand;
        CreateCliCommand _createCliCommand;
        INodeRegistry _nodeRegistry;
        RoundCliCommand _roundCliCommand;
        VariableCliCommand _variableCliCommand;
        CaptureCliCommand _captureCliCommand;
        IterationCliCommand _iterationCliCommand;
        RunCliCommand _runCliCommand;
        LoggerCliCommand _loggerCliCommand;
        WatchDogCliCommand _watchdogCliCommand;
        HttpClientCliCommand _httpClientCliCommand;
        readonly AppSettingsWritableOptions _appSettings;
        readonly IMetricsDataMonitor _lpsMonitoringEnroller;
        readonly CancellationTokenSource _cts;
        readonly ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationExecutionCommandStatusMonitor;
        readonly IPlaceholderResolverService _placeholderResolverService;
        readonly IMapper _mapper;
        readonly IEntityDiscoveryService _entityDiscoveryService;
        readonly ITestTriggerNotifier _testTriggerNotifier;
        ITestExecutionService _testExecutionService;
        #pragma warning disable CS8618
        public CommandLineManager(
            string[] command_args,
            ITestOrchestratorService testOrchestratorService,
            ITestExecutionService testExecutionService,
            INodeRegistry nodeRegistry,
            IClusterConfiguration clusterConfiguration,
            IEntityDiscoveryService entityDiscoveryService,
            ITestTriggerNotifier testTriggerNotifier,
            ILogger logger,
            IClientManager<HttpRequest, HttpResponse,
            IClientService<HttpRequest, HttpResponse>> httpClientManager,
            IClientConfiguration<HttpRequest> config,
            IWatchdog watchdog,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            AppSettingsWritableOptions appSettings,
            ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> httpIterationExecutionCommandStatusMonitor,
            IMetricsDataMonitor lpsMonitoringEnroller,
            IVariableManager variableManager,
            IPlaceholderResolverService placeholderResolverService,
            CancellationTokenSource cts)
        {
            _entityDiscoveryService = entityDiscoveryService;
            _testOrchestratorService = testOrchestratorService;
            _testExecutionService = testExecutionService;
            _nodeRegistry = nodeRegistry;
            _clusterConfiguration = clusterConfiguration;
            _testTriggerNotifier = testTriggerNotifier;
            _logger = logger;
            _command_args = command_args.Select(arg => arg.ToLowerInvariant()).ToArray();
            _config = config;
            _httpClientManager = httpClientManager;
            _watchdog = watchdog;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _appSettings = appSettings;
            _lpsMonitoringEnroller = lpsMonitoringEnroller;
            _httpIterationExecutionCommandStatusMonitor = httpIterationExecutionCommandStatusMonitor;
            _variableManager = variableManager;
            _cts = cts;
            _placeholderResolverService = placeholderResolverService;
            // Create the AutoMapper configuration
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DtoToCommandProfile(placeholderResolverService, string.Empty));
            });

            // Validate the configuration
            mapperConfig.AssertConfigurationIsValid();

            // Create the mapper instance
            _mapper = mapperConfig.CreateMapper();
            Configure();
        }
        private void Configure()
        {
            _rootCliCommand = new Command("lps", "Load, Performance and Stress Testing Command Tool.");
            _lpsCliCommand = new LpsCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _testOrchestratorService);
            _createCliCommand = new CreateCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _placeholderResolverService);
            _roundCliCommand = new RoundCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _placeholderResolverService);
            _iterationCliCommand = new IterationCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _placeholderResolverService);
            _runCliCommand = new RunCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _testOrchestratorService);
            _loggerCliCommand = new LoggerCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _appSettings.LPSFileLoggerOptions);
            _httpClientCliCommand = new HttpClientCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _appSettings.LPSHttpClientOptions);
            _watchdogCliCommand = new WatchDogCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _appSettings.LPSWatchdogOptions);
            _variableCliCommand = new VariableCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _placeholderResolverService);
            _captureCliCommand = new CaptureCliCommand(_rootCliCommand, _logger, _runtimeOperationIdProvider, _placeholderResolverService);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            string joinedCommand = string.Join(" ", _command_args);

            switch (joinedCommand.ToLowerInvariant())
            {
                case string cmd when cmd.StartsWith("create"):
                    _createCliCommand.SetHandler(cancellationToken);
                    break;
                case string cmd when cmd.StartsWith("round"):
                    _roundCliCommand.SetHandler(cancellationToken);
                    break;
                case string cmd when (cmd.StartsWith("iteration")):
                    _iterationCliCommand.SetHandler(cancellationToken);
                    break;
                case string cmd when cmd.StartsWith("variable"):
                    _variableCliCommand.SetHandler(cancellationToken);
                    break;
                case string cmd when cmd.StartsWith("capture"):
                    _captureCliCommand.SetHandler(cancellationToken);
                    break;
                case string cmd when cmd.StartsWith("run"):
                    _runCliCommand.SetHandler(cancellationToken);
                    break;
                case string cmd when cmd.StartsWith("logger"):
                    _loggerCliCommand.SetHandler(cancellationToken);
                    break;

                case string cmd when cmd.StartsWith("httpclient"):
                    _httpClientCliCommand.SetHandler(cancellationToken);
                    break;

                case string cmd when cmd.StartsWith("watchdog"):
                    _watchdogCliCommand.SetHandler(cancellationToken);
                    break;
                default:
                    _lpsCliCommand.SetHandler(cancellationToken);
                    break;
            }
            await _rootCliCommand.InvokeAsync(_command_args);
        }
    }
}
