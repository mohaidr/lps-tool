using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using LPS.Domain.Common.Interfaces;
using LPS.Domain;
using LPS.Infrastructure.Logger;
using LPS.Infrastructure.Common.Interfaces;
using LPS.Infrastructure.Nodes;
using static LPS.Protos.Shared.MetricsProtoService;
using LPS.Protos.Shared;

namespace LPS.Infrastructure.Monitoring.MetricsServices
{
    public class MetricsService : IMetricsService
    {
        private readonly ILogger _logger;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        private readonly ConcurrentDictionary<string, IList<IMetricCollector>> _metrics = new();
        private readonly IMetricsQueryService _metricsQueryService;
        private readonly INodeMetadata _nodeMetaData;
        private readonly IEntityDiscoveryService _entityDiscoveryService;
        private readonly MetricsProtoServiceClient _grpcClient;
        private readonly IClusterConfiguration _clusterConfiguration;
        public MetricsService(ILogger logger,
            INodeMetadata nodeMetaData,
            IEntityDiscoveryService entityDiscoveryService,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            IMetricsQueryService metricsQueryService,
            IClusterConfiguration clusterConfiguration)
        {
            _logger = logger;
            _entityDiscoveryService = entityDiscoveryService;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _metricsQueryService = metricsQueryService;
            _nodeMetaData = nodeMetaData;
            _clusterConfiguration = clusterConfiguration;

            if (_nodeMetaData.NodeType != Nodes.NodeType.Master)
            {
                var channel = GrpcChannel.ForAddress($"http://{_clusterConfiguration.MasterNodeIP}:{_clusterConfiguration.GRPCPort}");
                _grpcClient = new MetricsProtoServiceClient(channel);
            }
        }

        public async ValueTask<bool> TryIncreaseConnectionsCountAsync(Guid requestId, CancellationToken token)
        {
            if (_nodeMetaData.NodeType != Nodes.NodeType.Master)
            {
                var response = await _grpcClient.UpdateConnectionsAsync(new UpdateConnectionsRequest
                {
                    RequestId = requestId.ToString(),
                    Increase = true
                });
                return response.Success;
            }
            requestId = await DiscoverRequestIdOnLocalNode(requestId, token);
            if (requestId == Guid.Empty)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Failed to increase connections count because the requestId was empty", LPSLoggingLevel.Warning, token);
                return false;
            }
            await QueryMetricsAsync(requestId);
            var throughputMetrics = _metrics[requestId.ToString()]
                .Where(metric => metric.MetricType == LPSMetricType.Throughput);
            foreach (var metric in throughputMetrics)
            {
                ((IThroughputMetricCollector)metric).IncreaseConnectionsCount();
            }
            return true;
        }

        public async ValueTask<bool> TryDecreaseConnectionsCountAsync(Guid requestId, bool isSuccessful, CancellationToken token)
        {
            if (_nodeMetaData.NodeType != Nodes.NodeType.Master)
            {
                var response = await _grpcClient.UpdateConnectionsAsync(new UpdateConnectionsRequest
                {
                    RequestId = requestId.ToString(),
                    Increase = false,
                    IsSuccessful = isSuccessful
                });
                return response.Success;
            }
            requestId = await DiscoverRequestIdOnLocalNode(requestId, token);
            if (requestId == Guid.Empty)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Failed to decrease connections count because the requestId was empty", LPSLoggingLevel.Warning, token);
                return false;
            }
            await QueryMetricsAsync(requestId);
            var throughputMetrics = _metrics[requestId.ToString()]
                .Where(metric => metric.MetricType == LPSMetricType.Throughput);
            foreach (var metric in throughputMetrics)
            {
                ((IThroughputMetricCollector)metric).DecreseConnectionsCount(isSuccessful);
            }
            return true;

        }

        public async ValueTask<bool> TryUpdateResponseMetricsAsync(Guid requestId, HttpResponse.SetupCommand lpsResponse, CancellationToken token)
        {
            if (_nodeMetaData.NodeType != Nodes.NodeType.Master)
            {
                var response = await _grpcClient.UpdateResponseMetricsAsync(new UpdateResponseMetricsRequest
                {
                    RequestId = requestId.ToString(),
                    ResponseCode = (int)lpsResponse.StatusCode,
                    ResponseTime = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(lpsResponse.TotalTime)
                });
                return response.Success;
            }
            requestId = await DiscoverRequestIdOnLocalNode(requestId, token);
            if (requestId == Guid.Empty)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Failed to update response metrics because the requestId was empty", LPSLoggingLevel.Warning, token);
                return false;
            }
            await QueryMetricsAsync(requestId);
            var responseMetrics = _metrics[requestId.ToString()]
                .Where(metric => metric.MetricType == LPSMetricType.ResponseTime || metric.MetricType == LPSMetricType.ResponseCode);
            await Task.WhenAll(responseMetrics.Select(metric => ((IResponseMetricCollector)metric).UpdateAsync(lpsResponse)));
            return true;
        }

        public async ValueTask<bool> TryUpdateDataSentAsync(Guid requestId, double dataSize, double uploadTime, CancellationToken token)
        {
            if (_nodeMetaData.NodeType != Nodes.NodeType.Master)
            {
                var response = await _grpcClient.UpdateDataTransmissionAsync(new UpdateDataTransmissionRequest
                {
                    RequestId = requestId.ToString(),
                    DataSize = dataSize,
                    TimeTaken = uploadTime,
                    IsSent = true
                });
                return response.Success;
            }
            requestId = await DiscoverRequestIdOnLocalNode(requestId, token);
            if (requestId == Guid.Empty)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Failed to update data sent because the requestId was empty", LPSLoggingLevel.Warning, token);
                return false;
            }
            var dataTransmissionMetrics = await GetDataTransmissionMetricsAsync(requestId);
            foreach (var metric in dataTransmissionMetrics)
            {
                ((IDataTransmissionMetricCollector)metric).UpdateDataSent(dataSize, uploadTime, token);
            }
            return true;
        }

        public async ValueTask<bool> TryUpdateDataReceivedAsync(Guid requestId, double dataSize, double downloadTime, CancellationToken token)
        {
            if (_nodeMetaData.NodeType == Nodes.NodeType.Worker)
            {
                var response = await _grpcClient.UpdateDataTransmissionAsync(new UpdateDataTransmissionRequest
                {
                    RequestId = requestId.ToString(),
                    DataSize = dataSize,
                    TimeTaken = downloadTime,
                    IsSent = false
                });
                return response.Success;
            }
            requestId = await DiscoverRequestIdOnLocalNode(requestId, token);
            if (requestId == Guid.Empty)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Failed to update data received because the requestId was empty", LPSLoggingLevel.Warning, token);
                return false;
            }
            var dataTransmissionMetrics = await GetDataTransmissionMetricsAsync(requestId);
            foreach (var metric in dataTransmissionMetrics)
            {
                ((IDataTransmissionMetricCollector)metric).UpdateDataReceived(dataSize, downloadTime, token);
            }
            return true;
        }

        private async ValueTask<IEnumerable<IMetricCollector>> GetDataTransmissionMetricsAsync(Guid requestId)
        {
            await QueryMetricsAsync(requestId);
            return _metrics[requestId.ToString()]
                .Where(metric => metric.MetricType == LPSMetricType.DataTransmission);
        }

        private async Task QueryMetricsAsync(Guid requestId)
        {
            _metrics.TryAdd(requestId.ToString(),
                await _metricsQueryService.GetAsync(metric => metric.HttpIteration.HttpRequest.Id == requestId));
        }
        private async Task<Guid> DiscoverRequestIdOnLocalNode(Guid requestId, CancellationToken token)
        {
            var entityDiscoveryRecord = _entityDiscoveryService.Discover(r => r.RequestId == requestId).FirstOrDefault();
            if (entityDiscoveryRecord != null)
            {
                if (entityDiscoveryRecord.Node.Metadata.NodeType != Nodes.NodeType.Master)
                {
                    var fullyQualifiedName = entityDiscoveryRecord.FullyQualifiedName;
                    var matchingRequestId = _entityDiscoveryService.Discover(r => r.Node.Metadata.NodeType == Nodes.NodeType.Master && r.FullyQualifiedName == fullyQualifiedName).First().RequestId;
                    await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Found a matching HTTP request for '{requestId}' on the master node (ID: {matchingRequestId})", LPSLoggingLevel.Warning, token);

                    return matchingRequestId;
                }
                return requestId;
            }
            await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"No matching HTTP request found for '{requestId}' on the master node", LPSLoggingLevel.Warning, token);
            return Guid.Empty;
        }
    }
}
