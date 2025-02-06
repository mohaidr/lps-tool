using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using LPS.Infrastructure.Logger;
using LPS.Domain;
using LPS.UI.Common.Extensions;
using LPS.UI.Common;
using LPS.UI.Common.Options;
using Newtonsoft.Json;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Common;
using LPS.UI.Core.Host;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Infrastructure.Monitoring.Command;
using System.CommandLine;
using LPS.Infrastructure.Monitoring.Metrics;
using LPS.Infrastructure.LPSClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using LPS.UI.Core;
using Dashboard.Common;
using System.Reflection;
using LPS.Infrastructure.Common.Interfaces;
using LPS.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using LPS.Infrastructure.LPSClients.HeaderServices;
using LPS.Infrastructure.LPSClients.URLServices;
using LPS.Infrastructure.LPSClients.MessageServices;
using LPS.Infrastructure.LPSClients.ResponseService;
using LPS.Infrastructure.LPSClients.SampleResponseServices;
using LPS.Infrastructure.LPSClients.GlobalVariableManager;
using LPS.Infrastructure.LPSClients.PlaceHolderService;
using LPS.Infrastructure.LPSClients.SessionManager;
using LPS.Infrastructure.Monitoring.MetricsServices;


namespace LPS
{
    static class Startup
    {
        public static IHost ConfigureServices(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var contentRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    ArgumentNullException.ThrowIfNull(contentRoot);
                    webBuilder.UseContentRoot(contentRoot);

                    var webRoot = Path.Combine(contentRoot, "wwwroot");
                    webBuilder.UseWebRoot(webRoot);

                    // Load configuration to access DashboardConfigurationOptions for the Port setting
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile(AppConstants.AppSettingsFileLocation, optional: false, reloadOnChange: false)
                        .Build();

                    var dashboardOptions = configuration
                        .GetSection("LPSAppSettings:LPSDashboardConfiguration")
                        .Get<DashboardConfigurationOptions>();

                    var port = dashboardOptions?.Port ?? GlobalSettings.Port;

                    webBuilder.UseSetting("http_port", port.ToString())
                              .UseStartup<LPS.Dashboard.Startup>()
                              .UseStaticWebAssets();

                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenAnyIP(port);
                        serverOptions.AllowSynchronousIO = false;
                    })
                    .UseShutdownTimeout(TimeSpan.FromSeconds(30));
                })
                .ConfigureAppConfiguration((configBuilder) =>
                {
                    configBuilder.AddEnvironmentVariables();
                    string lpsAppSettings = AppConstants.AppSettingsFileLocation;
                    if (!File.Exists(lpsAppSettings))
                    {
                        // If the file doesn't exist, create it with default settings
                        CreateDefaultAppSettings(lpsAppSettings);
                    }
                    configBuilder.AddJsonFile(lpsAppSettings, optional: false, reloadOnChange: false);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IMetricsRepository, MetricsRepository>();
                    services.AddSingleton<IMetricsDataMonitor, MetricsDataMonitor>();
                    services.AddSingleton<IMetricsQueryService, MetricsQueryService>();
                    services.AddSingleton<ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration>, HttpIterationCommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration>>();
                    services.AddSingleton<AppSettingsWritableOptions>();
                    services.AddSingleton<CancellationTokenSource>();
                    services.AddSingleton<IConsoleLogger, ConsoleLogger>();
                    services.AddSingleton<ILogFormatter, LogFormatter>();
                    services.AddSingleton<ICacheService<string>, MemoryCacheService<string>>();
                    services.AddSingleton<ICacheService<long>, MemoryCacheService<long>>();
                    services.AddSingleton<ICacheService<object>, MemoryCacheService<object>>();
                    services.AddSingleton(new MemoryCache(new MemoryCacheOptions
                    {
                        SizeLimit = 1024
                    }));
                    services.AddSingleton<IClientManager<Domain.HttpRequest, Domain.HttpResponse, IClientService<Domain.HttpRequest, Domain.HttpResponse>>, HttpClientManager>();
                    services.AddSingleton<IRuntimeOperationIdProvider, RuntimeOperationIdProvider>();
                    services.AddSingleton<IHttpHeadersService, HttpHeadersService>();
                    services.AddSingleton<IMetricsService, MetricsService>();
                    services.AddSingleton<IUrlSanitizationService, UrlSanitizationService>();
                    services.AddSingleton<IMessageService, MessageService>();
                    services.AddSingleton<IResponseProcessingService, ResponseProcessingService>();
                    services.AddSingleton<IResponseProcessorFactory, ResponseProcessorFactory>();
                    services.AddSingleton<IPlaceholderResolverService, PlaceholderResolverService>();
                    services.AddSingleton<ISessionManager, SessionManager>();
                    services.AddSingleton<IVariableManager, VariableManager>();
                    services.ConfigureWritable<DashboardConfigurationOptions>(hostContext.Configuration.GetSection("LPSAppSettings:LPSDashboardConfiguration"), AppConstants.AppSettingsFileLocation);
                    services.ConfigureWritable<FileLoggerOptions>(hostContext.Configuration.GetSection("LPSAppSettings:LPSFileLoggerConfiguration"), AppConstants.AppSettingsFileLocation);
                    services.ConfigureWritable<WatchdogOptions>(hostContext.Configuration.GetSection("LPSAppSettings:LPSWatchdogConfiguration"), AppConstants.AppSettingsFileLocation);
                    services.ConfigureWritable<HttpClientOptions>(hostContext.Configuration.GetSection("LPSAppSettings:LPSHttpClientConfiguration"), AppConstants.AppSettingsFileLocation);
                    services.AddHostedService(p => p.ResolveWith<HostedService>(new { args }));
                    if (hostContext.HostingEnvironment.IsProduction())
                    {
                        //add production dependencies
                    }
                    else
                    {
                        // add development dependencies
                    }
                })
                .ConfigureLPSFileLogger()
                .ConfigureLPSWatchdog()
                .ConfigureLPSHttpClient()
                .UseConsoleLifetime(options => options.SuppressStatusMessages = true)
               .Build();

            return host;
        }

        private static void CreateDefaultAppSettings(string filePath)
        {
            // Get the directory path
            string directoryPath = Path.GetDirectoryName(filePath);

            // Check if the directory exists; if not, create it
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // You can customize this method to create the default settings
            // For example, you can create a basic JSON structure with default values
            var defaultSettings = new
            {
                LPSAppSettings = new AppSettings()
            };

            // Serialize the default settings object to JSON
            string jsonContent = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);

            // Write the JSON content to the specified file path
            File.WriteAllText(filePath, jsonContent);
        }
    }
}
