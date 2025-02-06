// MetricsDataService.cs
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Monitoring.MetricsServices
{
    public class MetricsQueryService(
        ILogger logger,
        IRuntimeOperationIdProvider runtimeOperationIdProvider,
        IMetricsRepository metricsRepository) : IMetricsQueryService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider ?? throw new ArgumentNullException(nameof(runtimeOperationIdProvider));
        private readonly IMetricsRepository _metricsRepository = metricsRepository ?? throw new ArgumentNullException(nameof(metricsRepository));

        public async ValueTask<List<IMetricCollector>> GetAsync(Func<IMetricCollector, bool> predicate)
        {
            try
            {
                return _metricsRepository.Data.Values
                    .SelectMany(metricsContainer => metricsContainer.Metrics)
                    .Where(predicate)
                    .ToList();
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Failed to get metrics.\n{ex}", LPSLoggingLevel.Error);
                return null;
            }
        }

        public async ValueTask<List<T>> GetAsync<T>(Func<T, bool> predicate) where T : IMetricCollector
        {
            try
            {
                return _metricsRepository.Data.Values
                    .SelectMany(metricsContainer => metricsContainer.Metrics.OfType<T>())
                    .Where(predicate)
                    .ToList();
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Failed to get metrics.\n{ex}", LPSLoggingLevel.Error);
                return null;
            }
        }
    }
}
