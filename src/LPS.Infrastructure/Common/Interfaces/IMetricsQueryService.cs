using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Common.Interfaces
{
    public interface IMetricsQueryService
    {
        ValueTask<List<IMetricCollector>> GetAsync(Func<IMetricCollector, bool> predicate);
        ValueTask<List<T>> GetAsync<T>(Func<T, bool> predicate) where T : IMetricCollector;
    }
}
