using Apis.Common;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Logger;
using LPS.Infrastructure.Nodes;
using LPS.UI.Common;
using LPS.UI.Common.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Core.Host
{
    public interface IDashboardService
    {
        public void Start();
        public Task WaitForDashboardRefreshAsync();
    }
    internal class DashboardService : IDashboardService
    {
        readonly ILogger _logger;
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;

        IOptions<DashboardConfigurationOptions> _dashboardConfig;
        IClusterConfiguration _clusterConfiguration;
        public DashboardService(
            ILogger logger,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            IOptions<DashboardConfigurationOptions> dashboardConfig,
            IClusterConfiguration clusterConfiguration)
        {
            _dashboardConfig = dashboardConfig;
            _clusterConfiguration = clusterConfiguration;
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
        }
        public void Start()
        {
            if (_dashboardConfig.Value.BuiltInDashboard.HasValue && _dashboardConfig.Value.BuiltInDashboard.Value)
            {
                var port = _dashboardConfig.Value?.Port ?? GlobalSettings.DefaultDashboardPort;
                var queryParams = $"refreshrate={_dashboardConfig.Value?.RefreshRate ?? 5}";
                OpenBrowser($"http://{_clusterConfiguration?.MasterNodeIP ?? "127.0.0.1"}:{port}?{queryParams}");
            }
        }
        public async Task WaitForDashboardRefreshAsync()
        {
            var refreshInterval = _dashboardConfig.Value.RefreshRate.HasValue ? _dashboardConfig.Value.RefreshRate.Value + 1 : 6;
            await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Please wait while the client refreshes its metrics. The program will shut down in approximately {refreshInterval} seconds.", LPSLoggingLevel.Information);
            await Task.Delay(TimeSpan.FromSeconds(refreshInterval));
        }
        private static void OpenBrowser(string url)
        {
            try
            {
                // Use platform-specific code to open the default web browser
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw new PlatformNotSupportedException("Unsupported platform.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening web browser: {ex.Message}");
            }
        }
    }
}
