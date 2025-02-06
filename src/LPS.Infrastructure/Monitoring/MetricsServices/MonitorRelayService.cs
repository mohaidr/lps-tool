using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Common.Interfaces;

namespace LPS.Infrastructure.Monitoring.MetricsServices
{
    public class MonitorRelayService : IMonitorRelayService
    {
        public void Monitor(HttpIteration httpIteration, string executionId)
        {
            throw new NotImplementedException();
        }

        public void RegisterCommand<TCommand, TEntity>(TCommand command, TEntity entity)
            where TCommand : IAsyncCommand<TEntity>
            where TEntity : IDomainEntity
        {
            throw new NotImplementedException();
        }

        public void Stop(HttpIteration httpIteration, string executionId)
        {
            throw new NotImplementedException();
        }

        public bool TryRegister(string roundName, HttpIteration httpIteration)
        {
            throw new NotImplementedException();
        }
    }
}
