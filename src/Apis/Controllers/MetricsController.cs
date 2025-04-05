using LPS.Domain.Common.Interfaces;
using LPS.Domain;
using LPS.Infrastructure.Common.Interfaces;
using LPS.Infrastructure.Monitoring.Command;
using LPS.Infrastructure.Monitoring.Metrics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Enums;
using HdrHistogram;
using LPS.Infrastructure.Nodes;

namespace Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController(
        LPS.Domain.Common.Interfaces.ILogger logger,
        ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> httpIterationCommandStatusMonitor,
        IRuntimeOperationIdProvider runtimeOperationIdProvider,
        IMetricsQueryService metricsQueryService) : ControllerBase
    {
        readonly LPS.Domain.Common.Interfaces.ILogger _logger = logger;
        readonly IRuntimeOperationIdProvider? _runtimeOperationIdProvider = runtimeOperationIdProvider;
        readonly ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationCommandStatusMonitor = httpIterationCommandStatusMonitor;
        readonly IMetricsQueryService _metricsQueryService = metricsQueryService;

        // MetricData class extended to hold Data Transmission metrics
        private class MetricData
        {
            public DateTime TimeStamp { get; set; }
            public string URL { get; set; }
            public string HttpMethod { get; set; }
            public string HttpVersion { get; set; }
            public string RoundName { get; set; }
            public Guid IterationId { get; set; }
            public string IterationName { get; set; }
            public string Endpoint { get; set; }
            public string ExecutionStatus { get; set; }
            public object ResponseBreakDownMetrics { get; set; }
            public object ResponseTimeMetrics { get; set; }
            public object ConnectionMetrics { get; set; }
            public object DataTransmissionMetrics { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var metricsList = new List<MetricData>();

            try
            {
                // Initiate asynchronous fetching of metrics
                var responseTimeMetricsTask = _metricsQueryService.GetAsync(metric => metric.MetricType == LPSMetricType.ResponseTime);
                var responseBreakDownMetricsTask = _metricsQueryService.GetAsync(metric => metric.MetricType == LPSMetricType.ResponseCode);
                var connectionsMetricsTask = _metricsQueryService.GetAsync(metric => metric.MetricType == LPSMetricType.Throughput);
                var dataTransmissionMetricsTask = _metricsQueryService.GetAsync(metric => metric.MetricType == LPSMetricType.DataTransmission);

                // Await all tasks to complete

                // Retrieve the results
                var responseTimeMetrics = await responseTimeMetricsTask;
                var responseBreakDownMetrics = await responseBreakDownMetricsTask;
                var connectionsMetrics = await connectionsMetricsTask;
                var dataTransmissionMetrics = await dataTransmissionMetricsTask; // Get data transmission metrics

                // Populate the metrics list
                await AddToList(responseTimeMetrics, "ResponseTime");
                await AddToList(responseBreakDownMetrics, "ResponseCode");
                await AddToList(connectionsMetrics, "ConnectionsCount");
                await AddToList(dataTransmissionMetrics, "DataTransmission"); // Add DataTransmission to the list
            }
            catch (Exception ex)
            {
                // Log the exception asynchronously
                if (_logger != null)
                {
                    await _logger.LogAsync(
                        _runtimeOperationIdProvider?.OperationId ?? "0000-0000-0000-0000",
                        $"Failed to retrieve metrics.\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}",
                        LPSLoggingLevel.Error);
                }

                return StatusCode(500, "An error occurred while retrieving metrics.");
            }

            return Ok(metricsList);

            // Helper action to add metrics to the list
            async ValueTask AddToList(IEnumerable<dynamic> metrics, string type)
            {
                foreach (var metric in metrics)
                {
                    var dimensionSet = await ((IMetricCollector)metric).GetDimensionSetAsync();

                    var statusList = await _httpIterationCommandStatusMonitor.Query(((IMetricCollector)metric).HttpIteration);
                    string status = statusList != null ? DetermineOverallStatus(statusList) : ExecutionStatus.Unkown.ToString();

                    var metricData = metricsList.FirstOrDefault(m => m.IterationId == ((IHttpDimensionSet)dimensionSet).IterationId);
                    if (metricData == null)
                    {
                        metricData = new MetricData
                        {
                            ExecutionStatus = status,
                            TimeStamp = ((IHttpDimensionSet)dimensionSet).TimeStamp,
                            RoundName = ((IHttpDimensionSet)dimensionSet).RoundName,
                            IterationId = ((IHttpDimensionSet)dimensionSet).IterationId,
                            IterationName = ((IHttpDimensionSet)dimensionSet).IterationName,
                            URL = ((IHttpDimensionSet)dimensionSet).URL,
                            HttpMethod = ((IHttpDimensionSet)dimensionSet).HttpMethod,
                            HttpVersion = ((IHttpDimensionSet)dimensionSet).HttpVersion,
                            Endpoint = $"{((IHttpDimensionSet)dimensionSet).IterationName} {((IHttpDimensionSet)dimensionSet).URL} HTTP/{((IHttpDimensionSet)dimensionSet).HttpVersion}"
                        };
                        metricsList.Add(metricData);
                    }
                    else if (metricData.ExecutionStatus != status)
                    {
                        metricData.ExecutionStatus = status;
                    }

                    switch (type)
                    {
                        case "ResponseTime":
                            metricData.ResponseTimeMetrics = await ((IMetricCollector)metric).GetDimensionSetAsync();
                            break;
                        case "ResponseCode":
                            metricData.ResponseBreakDownMetrics = await ((IMetricCollector)metric).GetDimensionSetAsync();
                            break;
                        case "ConnectionsCount":
                            metricData.ConnectionMetrics = await ((IMetricCollector)metric).GetDimensionSetAsync();
                            break;
                        case "DataTransmission":  // Handle data transmission metrics
                            metricData.DataTransmissionMetrics = await ((IMetricCollector)metric).GetDimensionSetAsync();
                            break;
                    }
                }
            }
        }

        private static string DetermineOverallStatus(List<ExecutionStatus> statuses)
        {
            if (statuses.Count == 0 || statuses.All(status => status == ExecutionStatus.PendingExecution))
                return "PendingExecution";
            if (statuses.Any(status => status == ExecutionStatus.Scheduled) && !statuses.Any(status => status == ExecutionStatus.Ongoing))
                return "Scheduled";
            if (statuses.Any(status => status == ExecutionStatus.Ongoing))
                return "Ongoing";
            if (statuses.Any(status => status == ExecutionStatus.Failed))
                return "Failed";
            if (statuses.Any(status => status == ExecutionStatus.Cancelled) && !statuses.Any(status => status == ExecutionStatus.Failed))
                return "Cancelled";
            if (statuses.All(status => status == ExecutionStatus.Completed))
                return "Completed";
            if (statuses.All(status => status == ExecutionStatus.Completed || status == ExecutionStatus.Paused) && statuses.Any(status => status == ExecutionStatus.Paused))
                return "Paused";
            return "Undefined"; // Default case, should ideally never be reached
        }
    }
}
