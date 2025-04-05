using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Common.Interfaces;
using LPS.Infrastructure.Logger;
using LPS.Infrastructure.LPSClients;
using LPS.Infrastructure.Watchdog;
using LPS.UI.Common.Options;
using LPS.UI.Core.LPSValidators;
using LPS.UI.Core.Build.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using LPS.Infrastructure.Nodes;
using FluentValidation;
using Apis.Common;
using LPS.Infrastructure.Common;

namespace LPS.UI.Common.Extensions
{


    public static class HostingExtensions
    {
        private const string DefaultLogEventId = "0000-0000-0000-0000";
        private const string FileLoggerConfigSection = "LPSAppSettings:FileLogger";
        private const string HttpClientConfigSection = "LPSAppSettings:HttpClient";
        private const string WatchdogConfigSection = "LPSAppSettings:Watchdog";
        private const string ClusterConfigSection = "LPSAppSettings:Cluster";

        public static IHostBuilder UseFileLogger(this IHostBuilder hostBuilder, FileLoggerOptions? lpsFileOptions = null)
        {
            var unused = hostBuilder.ConfigureServices((hostContext, services) =>
            {
                lpsFileOptions ??= hostContext.Configuration.GetSection(FileLoggerConfigSection).Get<FileLoggerOptions>();

                services.AddSingleton<ILogger>(serviceProvider =>
                {
                    FileLogger fileLogger;
                    var (validOptions, isValid) = ValidateOptions(
                        lpsFileOptions,
                        hostContext,
                        new FileLoggerValidator(),
                        FileLoggerConfigSection);

                    if (!isValid)
                    {
                        fileLogger = CreateDefaultFileLogger(serviceProvider);
                        LogConfigurationIssue(fileLogger, "Logger", FileLoggerConfigSection, validOptions == null);
                    }
                    else
                    {
                        var loggingConfig = validOptions != null
                            ? MapToLoggingConfiguration(validOptions)
                            : new LoggingConfiguration();
                        fileLogger = new FileLogger(
                            loggingConfig,
                            serviceProvider.GetRequiredService<IConsoleLogger>(),
                            serviceProvider.GetRequiredService<ILogFormatter>());
                    }

                    LogAppliedConfiguration(fileLogger, !isValid || validOptions == null, "Logger Options", fileLogger);
                    return fileLogger;
                });
            });

            return hostBuilder;
        }

        public static IHostBuilder UseHttpClient(this IHostBuilder hostBuilder, HttpClientOptions? lpsHttpClientOptions = null)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                lpsHttpClientOptions ??= hostContext.Configuration.GetSection(HttpClientConfigSection).Get<HttpClientOptions>();

                services.AddSingleton<IClientConfiguration<HttpRequest>>(serviceProvider =>
                {
                    var (validOptions, isValid) = ValidateOptions(
                        lpsHttpClientOptions,
                        hostContext,
                        new HttpClientValidator(),
                        HttpClientConfigSection);

                    var fileLogger = serviceProvider.GetRequiredService<ILogger>();
                    HttpClientConfiguration instance = HttpClientConfiguration.GetDefaultInstance();

                    if (!isValid)
                    {
                        LogConfigurationIssue(fileLogger, "Http Client", HttpClientConfigSection, validOptions == null);
                    }
                    else
                    {
                        instance = validOptions != null
                            ? MapToHttpClientConfiguration(validOptions)
                            : instance;
                    }

                    LogAppliedConfiguration(instance, !isValid || validOptions ==null, "LPS Http Client", fileLogger);
                    return instance;
                });
            });

            return hostBuilder;
        }

        public static IHostBuilder UseWatchdog(this IHostBuilder hostBuilder, WatchdogOptions? watchdogOptions = null)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                watchdogOptions ??= hostContext.Configuration.GetSection(WatchdogConfigSection).Get<WatchdogOptions>();

                services.AddSingleton<IWatchdog>(serviceProvider =>
                {
                    var (validOptions, isValid) = ValidateOptions(
                        watchdogOptions,
                        hostContext,
                        new WatchdogValidator(),
                        WatchdogConfigSection);

                    var fileLogger = serviceProvider.GetRequiredService<ILogger>();
                    var metricsService = serviceProvider.GetRequiredService<IMetricsQueryService>();
                    var operationIdProvider = serviceProvider.GetRequiredService<IRuntimeOperationIdProvider>();

                    Watchdog watchdog= Watchdog.GetDefaultInstance(fileLogger, operationIdProvider, metricsService);

                    if (!isValid)
                    {
                        LogConfigurationIssue(fileLogger, "Watchdog", WatchdogConfigSection, validOptions == null);
                    }
                    else
                    {
                        watchdog = validOptions != null
                            ? MapToWatchdog(validOptions, fileLogger, operationIdProvider, metricsService)
                            : watchdog;
                    }

                    LogAppliedConfiguration(watchdog, !isValid || validOptions == null, "Watchdog Configuration", fileLogger);
                    return watchdog;
                });
            });

            return hostBuilder;
        }

        public static IHostBuilder UseClusterConfiguration(this IHostBuilder hostBuilder, ClusterConfigurationOptions? lpsClusterOptions = null)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                lpsClusterOptions ??= hostContext.Configuration.GetSection(ClusterConfigSection).Get<ClusterConfigurationOptions>();

                services.AddSingleton<IClusterConfiguration>(serviceProvider =>
                {
                    var (validOptions, isValid) = ValidateOptions(
                        lpsClusterOptions,
                        hostContext,
                        new ClusteredConfigurationValidator(),
                        ClusterConfigSection);

                    var logger = serviceProvider.GetRequiredService<ILogger>();

                    //In case the cluster options were not provided, we assume that this is a single node test.
                    ClusterConfiguration instance = ClusterConfiguration.GetDefaultInstance(INode.NodeIP, GlobalSettings.DefaultGRPCPort);
                    if (!isValid)
                    {
                        LogConfigurationIssue(logger, "Cluster", ClusterConfigSection, validOptions == null);
                    }
                    else
                    {
                        instance = validOptions!=null 
                        ? MapToClusterConfiguration(validOptions)
                        : instance;
                    }

                    LogAppliedConfiguration(instance, !isValid || validOptions == null, "LPS Cluster Configuration", logger);

                    return instance;
                });
            });

            return hostBuilder;
        }

        #region Helper Methods

        private static (TOptions? validOptions, bool isValid) ValidateOptions<TOptions>(
            TOptions? providedOptions,
            HostBuilderContext hostContext,
            IValidator<TOptions> validator,
            string configSection)
            where TOptions : class
        {
            var options = providedOptions ?? hostContext.Configuration.GetSection(configSection).Get<TOptions>();
            if (options == null)
            {
                return (null, false);
            }

            var validationResult = validator.Validate(options);
            return validationResult.IsValid ? (options, true) : (null, false);
        }

        private static void LogConfigurationIssue(ILogger logger, string configType, string configSection, bool isMissing)
        {
            var message = isMissing
                ? $"{configSection} Section is missing from the settings file. Default settings will be applied."
                : "Options are not valid. Default settings will be applied. Fix the errors below.";
            logger.Log(DefaultLogEventId, message, LPSLoggingLevel.Warning);
        }

        private static void LogAppliedConfiguration<T>(T configuration, bool isDefault, string configName, ILogger _logger)
        {
            if (isDefault)
            {
                string jsonString = JsonSerializer.Serialize(configuration);
                AnsiConsole.MarkupLine($"[Magenta]Applied Default {configName}: {jsonString}[/]");
                _logger.Log(AppConstants.emptyLogId, $"Applied Default {configName}: {jsonString}", LPSLoggingLevel.Warning);
            }
        }

        private static FileLogger CreateDefaultFileLogger(IServiceProvider serviceProvider)
        {
            return new FileLogger(
                new LoggingConfiguration(),
                serviceProvider.GetRequiredService<IConsoleLogger>(),
                serviceProvider.GetRequiredService<ILogFormatter>());
        }

        private static LoggingConfiguration MapToLoggingConfiguration(FileLoggerOptions options)
        {
            #pragma warning disable CS8629 // Nullable value type may be null.
            return new LoggingConfiguration
            {
                LogFilePath = options.LogFilePath,
                LoggingLevel = options.LoggingLevel.Value,
                ConsoleLoggingLevel = options.ConsoleLogingLevel.Value,
                EnableConsoleLogging = options.EnableConsoleLogging.Value,
                DisableConsoleErrorLogging = options.DisableConsoleErrorLogging.Value,
                DisableFileLogging = options.DisableFileLogging.Value
            };
        }

        private static HttpClientConfiguration MapToHttpClientConfiguration(HttpClientOptions options)
        {
            #pragma warning disable CS8629 // Nullable value type may be null.
            return new HttpClientConfiguration(
                TimeSpan.FromSeconds(options.PooledConnectionLifeTimeInSeconds.Value),
                TimeSpan.FromSeconds(options.PooledConnectionIdleTimeoutInSeconds.Value),
                options.MaxConnectionsPerServer.Value,
                TimeSpan.FromSeconds(options.ClientTimeoutInSeconds.Value));
        }

        private static ClusterConfiguration MapToClusterConfiguration(ClusterConfigurationOptions options)
        {
            var GrpcPort = options.GRPCPort ?? GlobalSettings.DefaultGRPCPort;
            var masterIsWorker = options.MasterNodeIsWorker ?? true;
            return new ClusterConfiguration(options.MasterNodeIP, GrpcPort, masterIsWorker, options.ExpectedNumberOfWorkers.Value);
        }

        private static Watchdog MapToWatchdog(
            WatchdogOptions options,
            ILogger logger,
            IRuntimeOperationIdProvider operationIdProvider,
            IMetricsQueryService metricsService)
        {
            #pragma warning disable CS8629 // Nullable value type may be null.
            return new Watchdog(
                options.MaxMemoryMB.Value,
                options.MaxCPUPercentage.Value,
                options.CoolDownMemoryMB.Value,
                options.CoolDownCPUPercentage.Value,
                options.MaxConcurrentConnectionsCountPerHostName.Value,
                options.CoolDownConcurrentConnectionsCountPerHostName.Value,
                options.CoolDownRetryTimeInSeconds.Value,
                options.MaxCoolingPeriod.Value,
                options.ResumeCoolingAfter.Value,
                options.SuspensionMode.Value,
                logger,
                operationIdProvider,
                metricsService);
            #pragma warning restore CS8629 // Nullable value type may be null.
        }

        #endregion
    }
}
