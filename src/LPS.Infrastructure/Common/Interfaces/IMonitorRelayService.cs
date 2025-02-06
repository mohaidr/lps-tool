using LPS.Domain;
using LPS.Domain.Common.Interfaces;

namespace LPS.Infrastructure.Common.Interfaces
{
    public interface IMonitorRelayService
    {
        public bool TryRegister(string roundName, HttpIteration httpIteration);
        public void Monitor(HttpIteration httpIteration, string executionId);
        public void Stop(HttpIteration httpIteration, string executionId);
        public void RegisterCommand<TCommand, TEntity>(TCommand command, TEntity entity) where TCommand : IAsyncCommand<TEntity> where TEntity : IDomainEntity;
    }
}