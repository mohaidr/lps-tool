using LPS.UI.Common;
using LPS.UI.Core.Build.Services;
using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.UI.Common.Options;
using LPS.UI.Core.LPSValidators;
using LPS.Infrastructure.Common;
using Spectre.Console;
using LPS.UI.Core.LPSCommandLine;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.DTOs;
using LPS.Infrastructure.LPSClients.GlobalVariableManager;
using LPS.Infrastructure.Nodes;
using Grpc.Net.Client;
using LPS.Protos.Shared;
using LPS.Common.Interfaces;

namespace LPS.UI.Core.Host
{
    internal class HostedService(
        dynamic command_args,
        IClusterConfiguration clusterConfiguration,
        ITestOrchestratorService testOrchestratorService,
        IEntityDiscoveryService entityDiscoveryService,
        INodeMetadata nodeMetadata,
        INodeRegistry nodeRegistry,
        ILogger logger,
        IClientConfiguration<HttpRequest> config,
        IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> httpClientManager,
        IWatchdog watchdog,
        IRuntimeOperationIdProvider runtimeOperationIdProvider,
        IMetricsDataMonitor metricDataMonitor,
        IVariableManager variableManager,
        IPlaceholderResolverService placeholderResolverService,
        ICommandStatusMonitor<IAsyncCommand<HttpIteration>,
        HttpIteration> httpIterationExecutionCommandStatusMonitor,
        AppSettingsWritableOptions appSettings,
                ITestTriggerNotifier testTriggerNotifier,
                                ITestExecutionService testExecutionService,

        CancellationTokenSource cts) : IHostedService
    {
        readonly IClusterConfiguration _clusterConfiguration = clusterConfiguration;
        readonly INodeMetadata _nodeMetadata = nodeMetadata;
        readonly ITestTriggerNotifier _testTriggerNotifier = testTriggerNotifier;
        readonly INodeRegistry _nodeRegistry = nodeRegistry;
        readonly ILogger _logger = logger;
        readonly IEntityDiscoveryService _entityDiscoveryService = entityDiscoveryService;
        readonly IClientConfiguration<HttpRequest> _config = config;
        readonly IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> _httpClientManager = httpClientManager;
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider;
        readonly IWatchdog _watchdog = watchdog;
        readonly AppSettingsWritableOptions _appSettings = appSettings;
        readonly IMetricsDataMonitor _metricDataMonitor = metricDataMonitor;
        readonly ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationExecutionCommandStatusMonitor = httpIterationExecutionCommandStatusMonitor;
        readonly IVariableManager _variableManager = variableManager;
        readonly IPlaceholderResolverService _placeholderResolverService = placeholderResolverService;
        readonly ITestExecutionService _testExecutionService = testExecutionService;
        readonly ITestOrchestratorService _testOrchestratorService = testOrchestratorService;
        readonly string[] _command_args = command_args.args;
        readonly CancellationTokenSource _cts = cts;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await RegisterNodeAsync();
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, " -------------- LPS V1 - App execution has started  --------------", LPSLoggingLevel.Verbose);
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"is the correlation Id of this iteration", LPSLoggingLevel.Information);

                #pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                Console.CancelKeyPress += CancelKeyPressHandler;
                _ = WatchForCancellationAsync();

                if (_command_args != null && _command_args.Length > 0)
                {
                    var commandLineManager = new CommandLineManager(_command_args, _testOrchestratorService, _testExecutionService, _nodeRegistry, _clusterConfiguration, _entityDiscoveryService, _testTriggerNotifier, _logger, _httpClientManager, _config, _watchdog, _runtimeOperationIdProvider, _appSettings, _httpIterationExecutionCommandStatusMonitor, _metricDataMonitor, _variableManager, _placeholderResolverService, _cts);
                    await commandLineManager.RunAsync(_cts.Token);
                    await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, "Command execution has completed", LPSLoggingLevel.Verbose, cancellationToken);
                }
                else
                {
                    PlanDto planDto = new();
                    var manualBuild = new ManualBuild(new PlanValidator(planDto), _logger, _runtimeOperationIdProvider, _placeholderResolverService);
                    var plan = manualBuild.Build(ref planDto);
                    SavePlanToDisk(planDto);
                    AnsiConsole.MarkupLine($"[bold italic]You can use the command [blue]lps run {planDto.Name}.yaml[/] to execute the Plan[/]");

                }
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, " -------------- LPS V1 - App execution has completed  --------------", LPSLoggingLevel.Verbose, cancellationToken);
                await _logger.FlushAsync();
            }
            catch
            {
                await _nodeRegistry.GetLocalNode()
                .SetNodeStatus(Infrastructure.Nodes.NodeStatus.Failed);
            }
        }

        private async Task RegisterNodeAsync()
        {
            // keep this line for the worker nodes to register the nodes locally
            _nodeRegistry.RegisterNode(new Infrastructure.Nodes.Node(_nodeMetadata, _clusterConfiguration, _nodeRegistry)); // register locally
            // Create a gRPC Channel to the Server
            var channel = GrpcChannel.ForAddress($"http://{_clusterConfiguration.MasterNodeIP}:{_clusterConfiguration.GRPCPort}");

            // Create the gRPC Client
            var client = new NodeService.NodeServiceClient(channel);

            // Map Disks from `_nodeMetadata`
            var diskList = _nodeMetadata.Disks.Select(d => new LPS.Protos.Shared.DiskInfo
            {
                Name = d.Name,
                TotalSize = d.TotalSize,
                FreeSpace = d.FreeSpace
            }).ToList();

            // Map Network Interfaces from `_nodeMetadata`
            var networkList = _nodeMetadata.NetworkInterfaces.Select(n => new LPS.Protos.Shared.NetworkInfo
            {
                InterfaceName = n.InterfaceName,
                Type = n.Type,
                Status = n.Status,
                IpAddresses = { n.IpAddresses } // Converts List<string> to repeated field
            }).ToList();

            // Construct gRPC Request
            var nodeMetadata = new LPS.Protos.Shared.NodeMetadata
            {
                NodeName = _nodeMetadata.NodeName,
                NodeIp = _nodeMetadata.NodeIP,
                NodeType = _nodeMetadata.NodeType == Infrastructure.Nodes.NodeType.Master ? LPS.Protos.Shared.NodeType.Master : LPS.Protos.Shared.NodeType.Worker,
                Os = _nodeMetadata.OS,
                Architecture = _nodeMetadata.Architecture,
                Framework = _nodeMetadata.Framework,
                Cpu = _nodeMetadata.CPU,
                LogicalProcessors = _nodeMetadata.LogicalProcessors,
                TotalRam = _nodeMetadata.TotalRAM,
                Disks = { diskList }, // Assign mapped Disks
                NetworkInterfaces = { networkList } // Assign mapped NetworkInterfaces
            };

            // Call the gRPC Service
            RegisterNodeResponse response = await client.RegisterNodeAsync(nodeMetadata); // register on the master node

        }

        private static void SavePlanToDisk(PlanDto planDto)
        {
            var jsonContent = SerializationHelper.Serialize(planDto);
            File.WriteAllText($"{planDto.Name}.json", jsonContent);

            var yamlContent = SerializationHelper
                .SerializeToYaml(planDto);
            File.WriteAllText($"{planDto.Name}.yaml", yamlContent);

        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _logger.FlushAsync();
            await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, "--------------  LPS V1 - App Exited  --------------", LPSLoggingLevel.Verbose, cancellationToken);
            _programCompleted = true;

            var localNode = _nodeRegistry.GetLocalNode();
            await localNode.SetNodeStatus(Infrastructure.Nodes.NodeStatus.Stopped);

            if (!_cts.IsCancellationRequested 
                || localNode.Metadata.NodeType == Infrastructure.Nodes.NodeType.Master)
            {
                //TODO: Think about this approach
                while (_nodeRegistry.Query(n => n.NodeStatus == Infrastructure.Nodes.NodeStatus.Running
                || n.NodeStatus == Infrastructure.Nodes.NodeStatus.Pending).Count() > 0)
                {
                    await Task.Delay(1000);
                }
            }
        }

        private void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC
                || e.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                e.Cancel = true; // Prevent default process termination.
                AnsiConsole.MarkupLine("[yellow]Graceful shutdown requested (Ctrl+C/Break).[/]");
                RequestCancellation(); // Cancel the CancellationTokenSource.
                if (_nodeMetadata.NodeType == Infrastructure.Nodes.NodeType.Master)
                {

                    foreach (var node in _nodeRegistry.Query(n => n.Metadata.NodeType == Infrastructure.Nodes.NodeType.Worker))
                    {
                        var channel = GrpcChannel.ForAddress($"http://{node.Metadata.NodeIP}:{_clusterConfiguration.GRPCPort}");
                        var client = new NodeService.NodeServiceClient(channel);
                        client.CancelTest(new CancelTestRequest());

                    }
                }
            }
        }
        static bool _programCompleted;
        private async Task WatchForCancellationAsync()
        {
            while (!_cts.IsCancellationRequested && !_programCompleted)
            {
                if (Console.KeyAvailable) // Check for the Escape key
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        AnsiConsole.MarkupLine("[yellow]Graceful shutdown requested (Escape).[/]");
                        RequestCancellation(); // Cancel the CancellationTokenSource.
                        break; // Exit the loop
                    }
                }
                await Task.Delay(1000); // Poll every second
            }
        }

        private void RequestCancellation()
        {
            AnsiConsole.MarkupLine("[yellow]Gracefully shutting down the LPS local server[/]");
            _cts.Cancel();
        }

    }
}
