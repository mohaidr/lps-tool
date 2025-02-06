using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;
using LPS.Domain;
using LPS.Infrastructure.Logger;
using LPS.Infrastructure.Common.Interfaces;

namespace LPS.Infrastructure.Monitoring.MetricsServices
{
    public class MetricsService(ILogger logger,
        IRuntimeOperationIdProvider runtimeOperationIdProvider,
        IMetricsQueryService metricsQueryService) : IMetricsService
    {
        readonly ILogger _logger = logger;
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider;
        readonly ConcurrentDictionary<string, IList<IMetricCollector>> _metrics = new();
        readonly IMetricsQueryService _metricsQueryService = metricsQueryService;

        public async ValueTask<bool> TryIncreaseConnectionsCountAsync(Guid requestId, CancellationToken token)
        {
            try
            {
                await QueryMetricsAsync(requestId);
                var throughputMetrics = _metrics[requestId.ToString()]
                        .Where(metric => metric.MetricType == LPSMetricType.Throughput);
                foreach (var metric in throughputMetrics)
                {
                    ((IThroughputMetricCollector)metric).IncreaseConnectionsCount();
                }
                return true; // Avoids allocation, synchronous result
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId,
                                       $"Failed to increase connections metrics\n{(ex.InnerException != null ? ex.InnerException.Message : string.Empty)} \n\t  {ex.Message} \n  {ex.StackTrace}",
                                       LPSLoggingLevel.Error, token);
                return false;
            }
        }
        public async ValueTask<bool> TryDecreaseConnectionsCountAsync(Guid requestId, bool isSuccessful, CancellationToken token)
        {
            try
            {
                await QueryMetricsAsync(requestId);
                var throughputMetrics= _metrics[requestId.ToString()]
                        .Where(metric => metric.MetricType == LPSMetricType.Throughput);

                foreach (var metric in throughputMetrics)
                {
                    ((IThroughputMetricCollector)metric).DecreseConnectionsCount(isSuccessful);
                }
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId,
                                       $"Failed to decrease connections metrics\n{(ex.InnerException != null ? ex.InnerException.Message : string.Empty)} \n\t  {ex.Message} \n  {ex.StackTrace}",
                                       LPSLoggingLevel.Error, token);
                return false;
            }
        }
        public async ValueTask<bool> TryUpdateResponseMetricsAsync(Guid requestId, HttpResponse lpsResponse, CancellationToken token)
        {
            try
            {
                await QueryMetricsAsync(requestId);
                var responsMetrics = _metrics[requestId.ToString()].Where(metric => metric.MetricType == LPSMetricType.ResponseTime || metric.MetricType == LPSMetricType.ResponseCode);
                await Task.WhenAll(responsMetrics.Select(metric => ((IResponseMetricCollector)metric).UpdateAsync(lpsResponse)));
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Failed to update connections metrics\n{(ex.InnerException != null ? ex.InnerException.Message : string.Empty)} \n\t  {ex.Message} \n  {ex.StackTrace}", LPSLoggingLevel.Error, token);
                return false;
            }

        }
        private async ValueTask<IEnumerable<IMetricCollector>> GetDataTransmissionMetricsAsync(Guid requestId)
        {
            await QueryMetricsAsync(requestId);
            return _metrics[requestId.ToString()]
                    .Where(metric => metric.MetricType == LPSMetricType.DataTransmission);
        }
        public async ValueTask<bool> TryUpdateDataSentAsync(Guid requestId, double dataSize, double uploadTime, CancellationToken token)
        {
            try
            {
                var dataTransmissionMetrics = await GetDataTransmissionMetricsAsync(requestId);
                foreach (var metric in dataTransmissionMetrics)
                {
                    ((IDataTransmissionMetricCollector)metric).UpdateDataSent(dataSize, uploadTime, token);
                }
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId,
                                       $"Failed to update data sent metrics\n{(ex.InnerException != null ? ex.InnerException.Message : string.Empty)} \n\t  {ex.Message} \n  {ex.StackTrace}",
                                       LPSLoggingLevel.Error, token);
                return false;
            }
        }
        public async ValueTask<bool> TryUpdateDataReceivedAsync(Guid requestId, double dataSize, double downloadTime, CancellationToken token)
        {
            try
            {
                var dataTransmissionMetrics = await GetDataTransmissionMetricsAsync(requestId);
                foreach (var metric in dataTransmissionMetrics)
                {
                    ((IDataTransmissionMetricCollector)metric).UpdateDataReceived(dataSize, downloadTime, token);
                }
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId,
                                       $"Failed to update data received metrics\n{(ex.InnerException != null ? ex.InnerException.Message : string.Empty)} \n\t  {ex.Message} \n  {ex.StackTrace}",
                                       LPSLoggingLevel.Error, token);
                return false;
            }
        }
        private async Task QueryMetricsAsync(Guid requestId)
        {
            _metrics.TryAdd(requestId.ToString(),
                await _metricsQueryService.GetAsync(metric => metric.HttpIteration.HttpRequest.Id == requestId));
        }
    }
}
