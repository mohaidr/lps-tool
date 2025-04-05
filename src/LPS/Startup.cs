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
using Apis.Common;
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
using LPS.Infrastructure.Nodes;
using Microsoft.Extensions.Options;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics.Metrics;
using LPS.UI.Core.LPSValidators;
using FluentValidation;
using LPS.UI.Core.Services;
using LPS.Common.Services;
using LPS.Common.Interfaces;
using LPS.Infrastructure.GRPCClients.Factory;



namespace LPS
{
    static class Startup
    {
        public static IHost ConfigureServices(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                      .UseStartup<Apis.Startup>()
                      .UseStaticWebAssets();

                    var contentRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
                    ArgumentNullException.ThrowIfNull(contentRoot);
                    webBuilder.UseContentRoot(contentRoot);
                    var webRoot = Path.Combine(contentRoot, "wwwroot");
                    webBuilder.UseWebRoot(webRoot);

                    // Load configuration to access DashboardConfigurationOptions for the Port setting
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile(AppConstants.AppSettingsFileLocation, optional: false, reloadOnChange: false)
                        .Build();

                    var dashboardOptions = configuration
                        .GetSection("LPSAppSettings:Dashboard")
                        .Get<DashboardConfigurationOptions>();

                    var port =  dashboardOptions?.Port ?? GlobalSettings.DefaultDashboardPort;

                    var clusterOptions = configuration
                        .GetSection("LPSAppSettings:Cluster")
                        .Get<ClusterConfigurationOptions>();

                    var gRPCPort = (clusterOptions!=null && new ClusteredConfigurationValidator().Validate(clusterOptions).IsValid) ? clusterOptions.GRPCPort.Value : GlobalSettings.DefaultGRPCPort ;

                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenAnyIP(gRPCPort, listenOptions =>
                        {
                            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                        });
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
                    services.AddSingleton<ICustomGrpcClientFactory, CustomGrpcClientFactory>();
                    services.AddSingleton<IEntityDiscoveryService, EntityDiscoveryService>();
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
                    services.AddSingleton<INodeRegistry, NodeRegistry>();
                    services.AddSingleton<ITestTriggerNotifier, TestTriggerNotifier>();
                    services.AddSingleton<INodeMetadata, NodeMetadata>();
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
                    services.AddSingleton<ITestExecutionService, TestExecutionService>();
                    services.AddSingleton<ITestOrchestratorService, TestOrchestratorService>();
                    services.AddSingleton<IDashboardService, DashboardService>();
                    services.ConfigureWritable<DashboardConfigurationOptions>(hostContext.Configuration.GetSection("LPSAppSettings:Dashboard"), AppConstants.AppSettingsFileLocation);
                    services.ConfigureWritable<FileLoggerOptions>(hostContext.Configuration.GetSection("LPSAppSettings:FileLogger"), AppConstants.AppSettingsFileLocation);
                    services.ConfigureWritable<WatchdogOptions>(hostContext.Configuration.GetSection("LPSAppSettings:Watchdog"), AppConstants.AppSettingsFileLocation);
                    services.ConfigureWritable<HttpClientOptions>(hostContext.Configuration.GetSection("LPSAppSettings:HttpClient"), AppConstants.AppSettingsFileLocation);
                    services.AddHostedService(isp => isp.ResolveWith<HostedService>(new { args }));
                    if (hostContext.HostingEnvironment.IsProduction())
                    {
                        //add production dependencies
                    }
                    else
                    {
                        // add development dependencies
                    }
                })
                .UseFileLogger()
                .UseWatchdog()
                .UseHttpClient()
                .UseClusterConfiguration()
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
