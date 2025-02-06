using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Enums;
using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Monitoring.Command
{
    public class HttpIterationCommandStatusMonitor<TCommand, TEntity> : 
        ICommandStatusMonitor<TCommand, TEntity>
        where TCommand : IAsyncCommand<TEntity>
        where TEntity : HttpIteration
    {
        private readonly ConcurrentDictionary<TEntity, ConcurrentBag<TCommand>> _commandRegistry = new ConcurrentDictionary<TEntity, ConcurrentBag<TCommand>>();

        public void RegisterCommand(TCommand command, TEntity entity)
        {
            var commands = _commandRegistry.GetOrAdd(entity, (key) => new ConcurrentBag<TCommand>());
            commands.Add(command);
        }

        public void UnRegisterCommand(TCommand command, TEntity entity)
        {
            if (_commandRegistry.TryGetValue(entity, out var commands))
            {
                var newCommands = new ConcurrentBag<TCommand>(commands.Where(c => !c.Equals(command)));
                if (newCommands.IsEmpty)
                {
                    _commandRegistry.TryRemove(entity, out var _);
                }
                else
                {
                    _commandRegistry[entity] = newCommands;
                }
            }
        }

        public bool IsAnyCommandOngoing(TEntity entity)
        {
            if (_commandRegistry.TryGetValue(entity, out var commands))
            {
                return commands.Any(command => command.Status == ExecutionStatus.Ongoing);
            }
            return false;
        }
        public List<ExecutionStatus> GetAllStatuses(TEntity entity)
        {
            if (_commandRegistry.TryGetValue(entity, out var commands))
            {
                return commands.Select(command => command.Status).ToList();
            }
            return new List<ExecutionStatus>(); // Return an empty list if no commands are associated with the entity
        }
    }

}
