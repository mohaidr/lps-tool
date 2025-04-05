using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Common.Interfaces
{
    public interface IMetricsDataMonitor
    {
        public bool TryRegister(string roundName, HttpIteration lpsHttpIteration);
        public void Monitor(HttpIteration lpsHttpIteration);
        public void Monitor(Func<HttpIteration, bool> predicate);
        public void Stop(HttpIteration lpsHttpIteration);
        public void Stop(Func<HttpIteration, bool> predicate);
    }
}
