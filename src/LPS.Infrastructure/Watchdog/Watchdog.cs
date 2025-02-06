using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Common.Interfaces;
using LPS.Infrastructure.Monitoring.EventListeners;
using LPS.Infrastructure.Monitoring.Metrics;

namespace LPS.Infrastructure.Watchdog
{
    /// <summary>
    /// Defines the suspension modes for resource monitoring.
    /// </summary>
    public enum SuspensionMode
    {
        Any,
        All
    }

    /// <summary>
    /// Monitors system resources and manages cooling mechanisms based on defined thresholds.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Watchdog"/> class with specified configuration.
    /// </remarks>
    /// <param name="memoryLimitMB">Maximum allowed memory in MB.</param>
    /// <param name="cpuLimit">Maximum allowed CPU percentage.</param>
    /// <param name="coolDownMemoryMB">Memory threshold for cooldown.</param>
    /// <param name="coolDownCPUPercentage">CPU threshold for cooldown.</param>
    /// <param name="maxConcurrentConnectionsPerHostName">Maximum concurrent connections per host.</param>
    /// <param name="coolDownConcurrentConnectionsCountPerHostName">Concurrent connections threshold for cooldown.</param>
    /// <param name="coolDownRetryTimeInSeconds">Retry interval during cooldown.</param>
    /// <param name="maxCoolingPeriod">Maximum duration for cooling.</param>
    /// <param name="resumeCoolingAfter">Time after which cooling can be resumed.</param>
    /// <param name="suspensionMode">Mode of suspension (Any or All).</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationIdProvider">The operation ID provider.</param>
    public class Watchdog(
        double memoryLimitMB,
        double cpuLimit,
        double coolDownMemoryMB,
        double coolDownCPUPercentage,
        int maxConcurrentConnectionsPerHostName,
        int coolDownConcurrentConnectionsCountPerHostName,
        int coolDownRetryTimeInSeconds,
        int maxCoolingPeriod,
        int resumeCoolingAfter,
        SuspensionMode suspensionMode,
        ILogger logger,
        IRuntimeOperationIdProvider operationIdProvider,
        IMetricsQueryService metricsQueryService) : IWatchdog
    {
        public double MaxMemoryMB { get; } = memoryLimitMB;
        public double MaxCPUPercentage { get; } = cpuLimit;
        public double CoolDownMemoryMB { get; } = coolDownMemoryMB;
        public double CoolDownCPUPercentage { get; } = coolDownCPUPercentage;
        public SuspensionMode SuspensionMode { get; } = suspensionMode;
        public int CoolDownRetryTimeInSeconds { get; } = coolDownRetryTimeInSeconds;
        public int MaxConcurrentConnectionsCountPerHostName { get; } = maxConcurrentConnectionsPerHostName;
        public int CoolDownConcurrentConnectionsCountPerHostName { get; } = coolDownConcurrentConnectionsCountPerHostName;
        public int MaxCoolingPeriod { get; } = maxCoolingPeriod;
        public int ResumeCoolingAfter { get; } = resumeCoolingAfter;
        IMetricsQueryService _metricsQueryService = metricsQueryService;

        // Private fields
        private readonly ResourceEventListener _resourceListener = new ResourceEventListener();
        private readonly ILogger _logger = logger;
        private readonly IRuntimeOperationIdProvider _operationIdProvider = operationIdProvider;

        private ResourceState _resourceState = ResourceState.Cool;

        private bool _isResourceUsageExceeded;
        private bool _isResourceCoolingDown;

        // Synchronization
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        // Cooling state fields
        private bool _isCoolingStarted = false;
        private bool _isGCExecuted = false;
        private bool _isCoolingPaused = false;
        private readonly Stopwatch _maxCoolingStopwatch = new Stopwatch();
        private readonly Stopwatch _resetToCoolingStopwatch = new Stopwatch();

        private Watchdog(ILogger logger, IRuntimeOperationIdProvider operationIdProvider, IMetricsQueryService metricsQueryService)
            : this(
                  memoryLimitMB: 1000,
                  cpuLimit: 50,
                  coolDownMemoryMB: 500,
                  coolDownCPUPercentage: 30,
                  maxConcurrentConnectionsPerHostName: 1000,
                  coolDownConcurrentConnectionsCountPerHostName: 100,
                  coolDownRetryTimeInSeconds: 1,
                  maxCoolingPeriod: 60,
                  resumeCoolingAfter: 300,
                  suspensionMode: SuspensionMode.Any,
                  logger: logger,
                  operationIdProvider: operationIdProvider,
                  metricsQueryService: metricsQueryService)
        {
        }

        public static Watchdog GetDefaultInstance(ILogger logger, IRuntimeOperationIdProvider operationIdProvider,IMetricsQueryService metricsQueryService)
        {
            return new Watchdog(logger, operationIdProvider, metricsQueryService);
        }

        public async Task<ResourceState> BalanceAsync(string hostName, CancellationToken token = default)
        {
            bool semaphoreAcquired = false;
            try
            {
                await _semaphoreSlim.WaitAsync(token);
                semaphoreAcquired = true;
                if (_isCoolingPaused && _resetToCoolingStopwatch.Elapsed.TotalSeconds > ResumeCoolingAfter)
                {
                    await _logger.LogAsync(_operationIdProvider.OperationId, "Resuming cooling if needed", LPSLoggingLevel.Information, token);
                    _resetToCoolingStopwatch.Reset();
                    _isCoolingPaused = false;
                }

                await UpdateResourceUsageFlagAsync(hostName);
                await UpdateResourceCoolingFlagAsync(hostName);
                _resourceState = DetermineResourceState();

                while (_resourceState != ResourceState.Cool && !_isCoolingPaused && !token.IsCancellationRequested)
                {
                    if (!_isCoolingStarted)
                    {
                        await StartCoolingAsync(token);
                    }

                    if (_maxCoolingStopwatch.Elapsed.TotalSeconds > MaxCoolingPeriod)
                    {
                        await PauseCoolingAsync(token);
                        break;
                    }

                    if (!_isGCExecuted)
                    {
                        await ExecuteGarbageCollectionAsync(token);
                    }

                    await LogCoolingInitiationAsync(token);
                    await Task.Delay(TimeSpan.FromSeconds(CoolDownRetryTimeInSeconds), token);

                    await UpdateResourceUsageFlagAsync(hostName);
                    await UpdateResourceCoolingFlagAsync(hostName);
                    _resourceState = DetermineResourceState();
                }
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_operationIdProvider.OperationId,
                    $"Watchdog failed to balance resource usage.\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}",
                    LPSLoggingLevel.Error, token);
                _resourceState = ResourceState.Unknown;
            }
            finally
            {
                ResetCoolingState();
                if (semaphoreAcquired)
                {
                    _semaphoreSlim.Release();
                }
            }

            return _resourceState;
        }

        private async Task<int> GetHostActiveConnectionsCountAsync(string hostName)
        {
            try
            {
                var data = await _metricsQueryService
                    .GetAsync<ThroughputMetricCollector>(metric => metric.GetDimensionSetAsync<ThroughputDimensionSet>().Result?.URL?.Contains(hostName) == true);
                    return data.Sum(metric => metric.GetDimensionSetAsync<ThroughputDimensionSet>().Result?.ActiveRequestsCount ?? 0);
            }
            catch (Exception ex)
            {
               await _logger.LogAsync(_operationIdProvider.OperationId,
                    $"Failed to get active connections count.\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}",
                    LPSLoggingLevel.Error);
                return -1;
            }
        }

        private async Task UpdateResourceUsageFlagAsync(string hostName)
        {
            bool memoryExceeded = _resourceListener.MemoryUsageMB > MaxMemoryMB;
            bool cpuExceeded = _resourceListener.CPUPercentage >= MaxCPUPercentage;
            bool connectionsExceeded = (await GetHostActiveConnectionsCountAsync(hostName)) > MaxConcurrentConnectionsCountPerHostName;

            _isResourceUsageExceeded = SuspensionMode switch
            {
                SuspensionMode.Any => memoryExceeded || cpuExceeded || connectionsExceeded,
                SuspensionMode.All => memoryExceeded && cpuExceeded && connectionsExceeded,
                _ => false
            };
        }

        private async Task UpdateResourceCoolingFlagAsync(string hostName)
        {
            bool memoryExceedsCooldown = _resourceListener.MemoryUsageMB > CoolDownMemoryMB;
            bool cpuExceedsCooldown = _resourceListener.CPUPercentage >= CoolDownCPUPercentage;
            bool connectionsExceedsCooldown = (await GetHostActiveConnectionsCountAsync(hostName)) > CoolDownConcurrentConnectionsCountPerHostName;

            bool coolingCondition = SuspensionMode switch
            {
                SuspensionMode.Any => memoryExceedsCooldown || cpuExceedsCooldown || connectionsExceedsCooldown,
                SuspensionMode.All => memoryExceedsCooldown && cpuExceedsCooldown && connectionsExceedsCooldown,
                _ => false
            };

            _isResourceCoolingDown = coolingCondition && _resourceState != ResourceState.Cool;
        }

        private ResourceState DetermineResourceState()
        {
            return _isResourceUsageExceeded
                ? ResourceState.Hot
                : _isResourceCoolingDown
                    ? ResourceState.Cooling
                    : ResourceState.Cool;
        }

        private async Task StartCoolingAsync(CancellationToken token)
        {
            await _logger.LogAsync(_operationIdProvider.OperationId, "Cooling has started", LPSLoggingLevel.Information, token);
            _isCoolingStarted = true;
            _maxCoolingStopwatch.Start();
        }

        private async Task PauseCoolingAsync(CancellationToken token)
        {
            _isCoolingPaused = true;
            _resetToCoolingStopwatch.Start();
            await _logger.LogAsync(_operationIdProvider.OperationId, $"Pausing cooling for {ResumeCoolingAfter} seconds", LPSLoggingLevel.Information, token);
        }
        private async Task ExecuteGarbageCollectionAsync(CancellationToken token)
        {
            GC.Collect();
            _isGCExecuted = true;
            await _logger.LogAsync(_operationIdProvider.OperationId, "Garbage collection executed", LPSLoggingLevel.Warning, token);
        }

        private async Task LogCoolingInitiationAsync(CancellationToken token)
        {
            await _logger.LogAsync(_operationIdProvider.OperationId, "Resource utilization limit reached - initiating cooling...", LPSLoggingLevel.Warning, token);
        }

        private void ResetCoolingState()
        {
            _maxCoolingStopwatch.Reset();
            _isCoolingStarted = false;
            _isGCExecuted = false;
        }
    }
}
