// MetricsDataMonitor.cs
using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Infrastructure.Common.Interfaces;
using LPS.Infrastructure.Monitoring.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LPS.Infrastructure.Monitoring.MetricsServices
{
    public class MetricsDataMonitor(
        ILogger logger,
        IRuntimeOperationIdProvider runtimeOperationIdProvider,
        IMetricsRepository metricRepository,
        IMetricsQueryService metricsQueryService,
        ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> commandStatusMonitor) : IMetricsDataMonitor, IDisposable
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider ?? throw new ArgumentNullException(nameof(runtimeOperationIdProvider));
        private readonly IMetricsRepository _metricsRepository = metricRepository ?? throw new ArgumentNullException(nameof(metricRepository));
        private readonly IMetricsQueryService _metricsQueryService = metricsQueryService ?? throw new ArgumentNullException();
        ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _commandStatusMonitor = commandStatusMonitor ?? throw new ArgumentNullException();
        public bool TryRegister(string roundName, HttpIteration httpIteration)
        {
            try
            {
                if (_metricsRepository.Data.ContainsKey(httpIteration))
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"Iteration has already been registered. Below are the iteration details: \r\nRound:{roundName} \r\nIteration: {httpIteration.Name}", LPSLoggingLevel.Verbose);
                    return false;
                }

                var metrics = CreateMetricCollectors(roundName, httpIteration);
                var metricsContainer = new MetricsContainer(metrics);

                return _metricsRepository.Data.TryAdd(httpIteration, metricsContainer);
            }
            catch (Exception ex)
            {
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"Failed to register http iteration. Below are the exception details: \r\nRound:{roundName} \r\nIteration: {httpIteration.Name} \r\nException:{ex.Message} {ex.InnerException?.Message}", LPSLoggingLevel.Error);
                throw;
            }
        }

        private IReadOnlyList<IMetricCollector> CreateMetricCollectors(string roundName, HttpIteration httpIteration)
        {
            return new List<IMetricCollector>
            {
               new ResponseCodeMetricCollector(httpIteration, roundName, _logger, _runtimeOperationIdProvider),
               new DurationMetricCollector(httpIteration, roundName, _logger, _runtimeOperationIdProvider) ,
               new ThroughputMetricCollector(httpIteration, roundName, _logger, _runtimeOperationIdProvider) ,
               new DataTransmissionMetricCollector(httpIteration, roundName, _metricsQueryService, _logger, _runtimeOperationIdProvider)
            };
        }

        public void Monitor(HttpIteration httpIteration, string executionId)
        {
            bool iterationRegistered = _metricsRepository.Data.TryGetValue(httpIteration, out MetricsContainer metricsContainer);
            if (!iterationRegistered)
            {
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"Monitoring can't start. iteration {httpIteration.Name} has not been registered yet.", LPSLoggingLevel.Error);
                return;
            }

            foreach (var metric in metricsContainer.Metrics)
            {
                metric.Start();
            }
        }

        public void Stop(HttpIteration httpIteration, string executionId)
        {
            if (_metricsRepository.Data.TryGetValue(httpIteration, out var metricsContainer))
            {
                if (!_commandStatusMonitor.IsAnyCommandOngoing(httpIteration))
                {
                    foreach (var metric in metricsContainer.Metrics)
                    {
                        metric.Stop();
                    }
                }

            }
        }

        public void Dispose()
        {
            foreach (var monitoredIteration in _metricsRepository.Data.Values)
            {
                foreach (var metricCollector in monitoredIteration.Metrics)
                {
                    if (metricCollector is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}
