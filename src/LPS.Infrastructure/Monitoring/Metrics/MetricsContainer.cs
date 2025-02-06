using LPS.Domain;
using LPS.Infrastructure.Common.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Monitoring.Metrics
{
    public class MetricsContainer
    {
        public IReadOnlyList<IMetricCollector> Metrics { get; }

        public MetricsContainer(IReadOnlyList<IMetricCollector> metrics)
        {
            Metrics = metrics;
        }
    }
}
