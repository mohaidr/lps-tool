using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Common.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;

namespace LPS.Infrastructure.Monitoring.Metrics
{
    public class DataTransmissionMetricCollector : BaseMetricCollector, IDataTransmissionMetricCollector
    {
        private SpinLock _spinLock = new();
        private readonly string _roundName;
        private double _totalDataSent = 0;
        private double _totalDataReceived = 0;
        private int _requestsCount = 0;
        private double _totalDataUploadTime = 0;
        private double _totalDataDownloadTime = 0;
        private LPSDurationMetricDimensionSetProtected _dimensionSet;
        IMetricsQueryService _metricsQueryService;
        internal DataTransmissionMetricCollector(HttpIteration httpIteration, string roundName, IMetricsQueryService metricsQueryService, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
            : base(httpIteration, logger, runtimeOperationIdProvider)
        {
            _roundName = roundName;
            _httpIteration = httpIteration;
            _dimensionSet = new LPSDurationMetricDimensionSetProtected(_roundName, httpIteration.Id, httpIteration.Name, httpIteration.HttpRequest.HttpMethod, httpIteration.HttpRequest.Url.Url, httpIteration.HttpRequest.HttpVersion);
            _logger = logger;
            _metricsQueryService = metricsQueryService;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
        }

        protected override IDimensionSet DimensionSet => _dimensionSet;

        public override LPSMetricType MetricType => LPSMetricType.DataTransmission;
        public override void Start()
        {
            if (!IsStarted)
            {
                IsStarted = true;
                _logger.LogAsync("Start", "DataTransmissionMetricCollector started.", LPSLoggingLevel.Verbose).ConfigureAwait(false);
            }
        }

        public override void Stop()
        {
            if (IsStarted)
            {
                IsStarted = false;
                try
                {
                    _logger.LogAsync("Stop", "DataTransmissionMetricCollector stopped.", LPSLoggingLevel.Verbose).ConfigureAwait(false);
                }
                finally { }
            }
        }

        public void UpdateDataSent(double dataSize, double uploadTime, CancellationToken token = default)
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                _totalDataUploadTime += uploadTime;
                if (!IsStarted)
                {
                    throw new InvalidOperationException("Metric collector is stopped.");
                }

                // Update the total and count, then calculate the average
                _totalDataSent += dataSize;
                _requestsCount = _metricsQueryService.GetAsync<ThroughputMetricCollector>(m => m.HttpIteration.Id == this._dimensionSet.IterationId).Result
                    .Single()
                    .GetDimensionSetAsync<ThroughputDimensionSet>().Result
                    .RequestsCount;
                UpdateMetrics();
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        public void UpdateDataReceived(double dataSize, double downloadTime, CancellationToken token = default)
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (!IsStarted)
                {
                    throw new InvalidOperationException("Metric collector is stopped.");
                }

                // Update the total and count, then calculate the average
                _totalDataDownloadTime += downloadTime;
                _totalDataReceived += dataSize;
                _requestsCount = _metricsQueryService.GetAsync<ThroughputMetricCollector>(m => m.HttpIteration.Id == this._dimensionSet.IterationId).Result
                    .Single()
                    .GetDimensionSetAsync<ThroughputDimensionSet>().Result
                    .RequestsCount;

                UpdateMetrics();
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        readonly object lockObject = new();

        private void UpdateMetrics()
        {
            try
            {
                lock (lockObject)
                {
                    var totalDownloadSeconds = _totalDataDownloadTime / 1000;
                    var totalUploadSeconds = _totalDataUploadTime / 1000;
                    var totalSeconds = totalDownloadSeconds + totalUploadSeconds;

                    _dimensionSet.UpdateDataSent(_totalDataSent, (_requestsCount > 0 ? _totalDataSent / _requestsCount : 0), totalUploadSeconds > 0 ? _totalDataSent / totalUploadSeconds : 0, totalSeconds * 1000);
                    _dimensionSet.UpdateDataReceived(_totalDataReceived, _requestsCount > 0 ? _totalDataReceived / _requestsCount : 0, totalDownloadSeconds > 0 ? _totalDataReceived / totalDownloadSeconds : 0, totalSeconds * 1000);
                    _dimensionSet.UpdateAverageBytes(totalSeconds > 0 ? (_totalDataReceived + _totalDataSent) / totalSeconds : 0, totalSeconds * 1000);
                }
            }
            finally
            {

            }
        }

        private class LPSDurationMetricDimensionSetProtected : LPSDataTransmissionMetricDimensionSet
        {
            public LPSDurationMetricDimensionSetProtected(string roundName, Guid iterationId, string iterationName, string httpMethod, string url, string httpVersion)
            {
                RoundName = roundName;
                IterationId = iterationId;
                IterationName = iterationName;
                HttpMethod = httpMethod;
                URL = url;
                HttpVersion = httpVersion;
            }

            public void UpdateDataSent(double totalDataSent, double averageDataSentPerRequest, double averageDataSentPerSecond, double totalDataTransmissionTimeInMilliseconds)
            {
                TimeStamp = DateTime.UtcNow;
                DataSent = totalDataSent;
                AverageDataSent = averageDataSentPerRequest;
                AverageDataSentPerSecond = averageDataSentPerSecond;
                TotalDataTransmissionTimeInMilliseconds = totalDataTransmissionTimeInMilliseconds;
            }

            public void UpdateDataReceived(double totalDataReceived, double averageDataReceivedPerRequest, double averageDataReceivedPerSecond, double totalDataTransmissionTimeInMilliseconds)
            {
                TimeStamp = DateTime.UtcNow;
                DataReceived = totalDataReceived;
                AverageDataReceived = averageDataReceivedPerRequest;
                AverageDataReceivedPerSecond = averageDataReceivedPerSecond;
                TotalDataTransmissionTimeInMilliseconds = totalDataTransmissionTimeInMilliseconds;
            }

            public void UpdateAverageBytes(double averageBytesPerSecond, double totalDataTransmissionTimeInMilliseconds)
            {
                TimeStamp = DateTime.UtcNow;
                AverageBytesPerSecond = averageBytesPerSecond;
                TotalDataTransmissionTimeInMilliseconds = totalDataTransmissionTimeInMilliseconds;
            }

        }
    }

    public class LPSDataTransmissionMetricDimensionSet : IHttpDimensionSet
    {

        [JsonIgnore]
        public DateTime TimeStamp { get; protected set; }
        public double TotalDataTransmissionTimeInMilliseconds { get; protected set; }
        [JsonIgnore]
        public string RoundName { get; protected set; }
        [JsonIgnore]
        public Guid IterationId { get; protected set; }
        [JsonIgnore]
        public string IterationName { get; protected set; }
        [JsonIgnore]
        public string URL { get; protected set; }
        [JsonIgnore]
        public string HttpMethod { get; protected set; }
        [JsonIgnore]
        public string HttpVersion { get; protected set; }
        public double DataSent { get; protected set; }
        public double DataReceived { get; protected set; }
        public double AverageDataSent { get; protected set; }
        public double AverageDataReceived { get; protected set; }
        public double AverageDataSentPerSecond { get; protected set; }
        public double AverageDataReceivedPerSecond { get; protected set; }
        public double AverageBytesPerSecond { get; protected set; }
    }
}
