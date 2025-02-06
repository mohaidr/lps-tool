using LPS.UI.Common;
using System.Threading;
using System.Threading.Tasks;
using LPS.UI.Core.Build.Services;
using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using System.IO;
using LPS.UI.Common.Options;
using LPS.UI.Core.LPSValidators;
using LPS.Infrastructure.Common;
using Spectre.Console;
using LPS.UI.Core.LPSCommandLine;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Infrastructure.Monitoring.Metrics;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using LPS.DTOs;
using LPS.Infrastructure.LPSClients.GlobalVariableManager;
using LPS.Infrastructure.LPSClients.PlaceHolderService;

namespace LPS.UI.Core.Host
{
    internal class HostedService(
        dynamic command_args,
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
        CancellationTokenSource cts) : IHostedService
    {
        readonly ILogger _logger = logger;
        readonly IClientConfiguration<HttpRequest> _config = config;
        readonly IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> _httpClientManager = httpClientManager;
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider;
        readonly IWatchdog _watchdog = watchdog;
        readonly AppSettingsWritableOptions _appSettings = appSettings;
        readonly IMetricsDataMonitor _metricDataMonitor = metricDataMonitor;
        readonly ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationExecutionCommandStatusMonitor = httpIterationExecutionCommandStatusMonitor;
        readonly IVariableManager _variableManager = variableManager;
        readonly IPlaceholderResolverService _placeholderResolverService = placeholderResolverService;  
        readonly string[] _command_args = command_args.args;
        readonly CancellationTokenSource _cts = cts;
        static bool _cancelRequested;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, " -------------- LPS V1 - App execution has started  --------------", LPSLoggingLevel.Verbose);
            await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"is the correlation Id of this iteration", LPSLoggingLevel.Information);

            #pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            Console.CancelKeyPress += CancelKeyPressHandler;
            _ = WatchForCancellationAsync();



            if (_command_args != null && _command_args.Length > 0)
            {
                  var commandLineManager = new CommandLineManager(_command_args, _logger, _httpClientManager, _config, _watchdog, _runtimeOperationIdProvider, _appSettings, _httpIterationExecutionCommandStatusMonitor, _metricDataMonitor, _variableManager, _placeholderResolverService, _cts);
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
        }

        private void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC || e.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                e.Cancel = true; // Prevent default process termination.
                AnsiConsole.MarkupLine("[yellow]Graceful shutdown requested (Ctrl+C/Break).[/]");
                RequestCancellation(); // Cancel the CancellationTokenSource.
            }
        }
        static bool _programCompleted;
        private async Task WatchForCancellationAsync()
        {
            while (!_cancelRequested && !_programCompleted)
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
            if (!_cancelRequested)
            {
                _cancelRequested = true;
                AnsiConsole.MarkupLine("[yellow]Gracefully shutting down the LPS local server[/]");
                _cts.Cancel();
            }
        }

    }
}
