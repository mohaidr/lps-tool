using HdrHistogram;
using LPS.Domain;
using LPS.Infrastructure.Common;
using LPS.Infrastructure.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using System.Diagnostics.Tracing;
using LPS.Infrastructure.Monitoring.EventSources;
using LPS.Domain.Common.Interfaces;
using System.Diagnostics;
using System.Timers;
using System.Text.Json.Serialization;
using System.Net;
namespace LPS.Infrastructure.Monitoring.Metrics
{

    public class DurationMetricCollector : BaseMetricCollector, IResponseMetricCollector
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly LPSDurationMetricDimensionSetProtected _dimensionSet;
        readonly LongHistogram _histogram;
        readonly ResponseMetricEventSource _eventSource;
        internal DurationMetricCollector(HttpIteration httpIteration, string roundName, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider) : base (httpIteration, logger, runtimeOperationIdProvider)
        {
            _httpIteration = httpIteration;
            _eventSource = ResponseMetricEventSource.GetInstance(_httpIteration);
            _dimensionSet = new LPSDurationMetricDimensionSetProtected(roundName, _httpIteration.Id, httpIteration.Name, httpIteration.HttpRequest.HttpMethod, httpIteration.HttpRequest.Url.Url, httpIteration.HttpRequest.HttpVersion);
            _histogram = new LongHistogram(1, 1000000, 3);
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
        }
        protected override IDimensionSet DimensionSet => _dimensionSet;

        public override LPSMetricType MetricType => LPSMetricType.ResponseTime;
        public async Task<IResponseMetricCollector> UpdateAsync(HttpResponse response)
        {
            await _semaphore.WaitAsync();
            try
            {
                _dimensionSet.Update(response.TotalTime.TotalMilliseconds, _histogram);
                _eventSource.WriteResponseTimeMetrics(response.TotalTime.TotalMilliseconds);
            }
            finally
            {
                _semaphore.Release();
            }
            return this;
        }

        public IResponseMetricCollector Update(HttpResponse httpResponse)
        {
            return UpdateAsync(httpResponse).Result;
        }

        public override void Stop()
        {
            if (IsStarted)
            {
                IsStarted = false;
            }
        }
        public override void Start()
        {
            if (!IsStarted)
            {
                IsStarted = true;
            }
        }

        private class LPSDurationMetricDimensionSetProtected : LPSDurationMetricDimensionSet
        {
            public LPSDurationMetricDimensionSetProtected(string roundName, Guid iterationId, string iterationName, string httpMethod, string url, string httpVersion) {
                IterationId = iterationId;
                RoundName = roundName;
                IterationName = iterationName;
                HttpMethod = httpMethod;
                URL = url;
                HttpVersion = httpVersion;
            }
            public void Update(double responseTime, LongHistogram histogram)
            {
                double averageDenominator = AverageResponseTime != 0 ? (SumResponseTime / AverageResponseTime) + 1 : 1;
                TimeStamp = DateTime.UtcNow;
                MaxResponseTime = Math.Max(responseTime, MaxResponseTime);
                MinResponseTime = MinResponseTime == 0 ? responseTime : Math.Min(responseTime, MinResponseTime);
                SumResponseTime = SumResponseTime + responseTime;
                AverageResponseTime = SumResponseTime / averageDenominator;
                histogram.RecordValue((long)responseTime);
                P10ResponseTime = histogram.GetValueAtPercentile(10);
                P50ResponseTime = histogram.GetValueAtPercentile(50);
                P90ResponseTime = histogram.GetValueAtPercentile(90);
            }
        }
    }

    public class LPSDurationMetricDimensionSet: IHttpDimensionSet
    {
        [JsonIgnore]
        public DateTime TimeStamp { get; protected set; }
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
        public double SumResponseTime { get; protected set; }
        public double AverageResponseTime { get; protected set; }
        public double MinResponseTime { get; protected set; }
        public double MaxResponseTime { get; protected set; }
        public double P90ResponseTime { get; protected set; }
        public double P50ResponseTime { get; protected set; }
        public double P10ResponseTime { get; protected set; }
    }
}
