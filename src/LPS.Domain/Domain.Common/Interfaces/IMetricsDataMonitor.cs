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
        public void Monitor(HttpIteration lpsHttpIteration, string executionId);
        public void Stop(HttpIteration lpsHttpIteration, string executionId);
    }
}
