using LPS.Domain.Common.Interfaces;
using LPS.Domain;
using LPS.Infrastructure.Common.Interfaces;
using LPS.Infrastructure.Common;
using LPS.Infrastructure.Monitoring.EventSources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace LPS.Infrastructure.Monitoring.Metrics
{
    public class ResponseCodeMetricCollector : BaseMetricCollector, IResponseMetricCollector
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        readonly ResponseMetricEventSource _eventSource;

        internal ResponseCodeMetricCollector(HttpIteration httpIteration, string roundName, ILogger logger , IRuntimeOperationIdProvider runtimeOperationIdProvider) : base(httpIteration, logger, runtimeOperationIdProvider)
        {
            _httpIteration = httpIteration;
            _eventSource = ResponseMetricEventSource.GetInstance(_httpIteration);
            _dimensionSet = new ProtectedResponseCodeDimensionSet(roundName, _httpIteration.Id, _httpIteration.Name, _httpIteration.HttpRequest.HttpMethod, _httpIteration.HttpRequest.Url.Url, _httpIteration.HttpRequest.HttpVersion);
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
        }

        protected override IDimensionSet DimensionSet => _dimensionSet;

        public override LPSMetricType MetricType => LPSMetricType.ResponseCode;
        private ProtectedResponseCodeDimensionSet _dimensionSet { get; set; }

        public IResponseMetricCollector Update(HttpResponse response)
        {
            return UpdateAsync(response).Result;
        }

        public async Task<IResponseMetricCollector> UpdateAsync(HttpResponse response)
        {
            await _semaphore.WaitAsync();
            try
            {
                _dimensionSet.Update(response);
                _eventSource.WriteResponseBreakDownMetrics(response.StatusCode);
            }
            finally
            {
                _semaphore.Release();
            }
            return this;
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

        private class ProtectedResponseCodeDimensionSet : ResponseCodeDimensionSet
        {
            public ProtectedResponseCodeDimensionSet(string roundName, Guid iterationId, string iterationName, string httpMethod, string url, string httpVersion)
            {
                IterationId = iterationId;
                RoundName = roundName;
                IterationName = iterationName;
                HttpMethod = httpMethod;
                URL = url;
                HttpVersion = httpVersion;
            }

            public void Update(HttpResponse response)
            {
                var existingSummary = _responseSummaries.FirstOrDefault(rs => rs.HttpStatusCode == ((int)response.StatusCode).ToString() && rs.HttpStatusReason == response.StatusMessage);
                if (existingSummary != null)
                {
                    existingSummary.Count += 1;
                }
                else
                {
                    var summary = new ResponseSummary(
                        ((int)response.StatusCode).ToString(),
                        response.StatusMessage,
                        1
                    );
                    _responseSummaries.Add(summary);
                }

                TimeStamp = DateTime.UtcNow;
            }
        }
    }
    public class ResponseSummary(string httpStatusCode, string httpStatusReason, int count)
    {
        public string HttpStatusCode { get; private set; } = httpStatusCode;
        public string HttpStatusReason { get; private set; } = httpStatusReason;
        public int Count { get; set; } = count;
    }
    public class ResponseCodeDimensionSet : IHttpDimensionSet
    {

        public ResponseCodeDimensionSet()
        {
            _responseSummaries = new ConcurrentBag<ResponseSummary>();
        }
        [JsonIgnore]
        public string RoundName { get; protected set; }
        [JsonIgnore]
        public DateTime TimeStamp { get; protected set; }
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
        protected ConcurrentBag<ResponseSummary> _responseSummaries { get; set; }

        public IList<ResponseSummary> ResponseSummary => _responseSummaries.ToList();
    }
}
