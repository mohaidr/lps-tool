using LPS.Domain;
using LPS.Infrastructure.Common.Interfaces;
using System.Collections.Concurrent;


namespace LPS.Infrastructure.Monitoring.Metrics
{
    public class MetricsRepository : IMetricsRepository
    {
        public ConcurrentDictionary<HttpIteration, MetricsContainer> Data { get; } = new ConcurrentDictionary<HttpIteration, MetricsContainer>();
    }
}
