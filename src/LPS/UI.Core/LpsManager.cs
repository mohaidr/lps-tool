using Dashboard.Common;
using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Infrastructure.Monitoring.Metrics;
using LPS.UI.Common.Options;
using LPS.UI.Core.Host;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.UI.Core
{
    internal class LPSManager
    {
        readonly ILogger _logger;
        readonly IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> _httpClientManager;
        readonly IClientConfiguration<HttpRequest> _config;
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        readonly IWatchdog _watchdog;
        readonly IMetricsDataMonitor _lpsMonitoringEnroller;
        readonly ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationExecutionCommandStatusMonitor;
        readonly CancellationTokenSource _cts;
        IOptions<DashboardConfigurationOptions> _dashboardConfig;
        internal LPSManager(ILogger logger,
                IClientManager<HttpRequest, HttpResponse,IClientService<HttpRequest, HttpResponse>> httpClientManager,
                IClientConfiguration<HttpRequest> config,
                IWatchdog wtahcdog,
                IRuntimeOperationIdProvider runtimeOperationIdProvider,
                ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> httpIterationExecutionCommandStatusMonitor,
                IMetricsDataMonitor lpsMonitoringEnroller,
                IOptions<DashboardConfigurationOptions> dashboardConfig,
                CancellationTokenSource cts)
        {
            _logger = logger;
            _httpClientManager = httpClientManager;
            _config = config;
            _watchdog = wtahcdog;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _httpIterationExecutionCommandStatusMonitor = httpIterationExecutionCommandStatusMonitor;
            _dashboardConfig = dashboardConfig;
            _lpsMonitoringEnroller = lpsMonitoringEnroller;
            _cts = cts;
        }
        public async Task RunAsync(Plan plan)
        {
            try
            {
                var count = plan.GetReadOnlyRounds().Count();
                if (plan != null && plan.IsValid && count > 0)
                {
                    if (_dashboardConfig.Value.BuiltInDashboard.HasValue && _dashboardConfig.Value.BuiltInDashboard.Value)
                    {
                        var port = _dashboardConfig.Value?.Port ?? GlobalSettings.Port;
                        var queryParams = $"refreshrate={_dashboardConfig.Value?.RefreshRate ?? 5}";
                        Host.Dashboard.Start(port, queryParams);
                    }
                    await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Plan '{plan?.Name}' execution has started", LPSLoggingLevel.Information);
                    await new Plan.ExecuteCommand(_logger, _watchdog, _runtimeOperationIdProvider, _httpClientManager, _config, _httpIterationExecutionCommandStatusMonitor, _lpsMonitoringEnroller, _cts)
                        .ExecuteAsync(plan);
                    await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Plan '{plan?.Name}' execution has completed", LPSLoggingLevel.Information);
                }
            }
            finally 
            {
                    var refreshInterval = _dashboardConfig.Value.RefreshRate.HasValue ? _dashboardConfig.Value.RefreshRate.Value + 1 : 6;
                    await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Please hold on; we allow time for the client to refresh the metrics. Expected shutdown in {refreshInterval} seconds.", LPSLoggingLevel.Information);
                    await Task.Delay(TimeSpan.FromSeconds(refreshInterval));
            }
        }

    }
}
