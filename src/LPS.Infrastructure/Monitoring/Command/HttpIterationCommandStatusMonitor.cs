using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Enums;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Infrastructure.GRPCClients;
using LPS.Infrastructure.GRPCClients.Factory;
using LPS.Infrastructure.Nodes;
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
        IEntityDiscoveryService _entityDiscoveryService;
        INodeRegistry _nodeRegistry;
        INodeMetadata _nodeMetadata;
        ICustomGrpcClientFactory _grpcClientFactory;
        public HttpIterationCommandStatusMonitor(
            IEntityDiscoveryService entityDiscoveryService,
            INodeRegistry nodeRegistry,
            INodeMetadata nodeMetadata,
            ICustomGrpcClientFactory grpcClientFactory)
        {
            _entityDiscoveryService = entityDiscoveryService;
            _nodeRegistry = nodeRegistry;
            _nodeMetadata = nodeMetadata;
            _grpcClientFactory = grpcClientFactory;
        }

        private readonly ConcurrentDictionary<TEntity, ConcurrentBag<TCommand>> _commandRegistry = new ConcurrentDictionary<TEntity, ConcurrentBag<TCommand>>();

        public void Register(TCommand command, TEntity entity)
        {
            var commands = _commandRegistry.GetOrAdd(entity, (key) => new ConcurrentBag<TCommand>());
            commands.Add(command);
        }

        public void UnRegister(TCommand command, TEntity entity)
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

        public async ValueTask<bool> IsAnyCommandOngoing(TEntity entity)
        {
            if (_commandRegistry.TryGetValue(entity, out var commands))
            {
                List<ExecutionStatus> remoteCommandsStatuses = await GetRemoteStatusesAsync(entity);
                return (commands.Any(command => command.Status == ExecutionStatus.Ongoing) || remoteCommandsStatuses.Any(status => status== ExecutionStatus.Ongoing));
            }
            return false;
        }

        public async ValueTask<List<ExecutionStatus>> Query(TEntity entity)
        {
            List<ExecutionStatus> remoteCommandsStatuses = await GetRemoteStatusesAsync(entity); ;

            if (_commandRegistry.TryGetValue(entity, out var commands))
            {
                return commands.Select(command => command.Status).Concat(remoteCommandsStatuses).ToList();
            }
            return []; // Return an empty list if no commands are associated with the entity
        }

        public async ValueTask<Dictionary<TEntity, IList<ExecutionStatus>>> Query(Func<TEntity, bool> predicate)
        {
            Dictionary<TEntity, IList<ExecutionStatus>> entityStatuses = [];
            var entities = _commandRegistry.Keys.Where(predicate).ToList();
            foreach (var entity in entities)
            {
                var commands = _commandRegistry[entity];
                List<ExecutionStatus> remoteCommandsStatuses = await GetRemoteStatusesAsync(entity); ;

                entityStatuses.Add(entity, commands.Select(command => command.Status).Concat(remoteCommandsStatuses).ToList());
            }
            return entityStatuses;
        }

        private async ValueTask<List<ExecutionStatus>> GetRemoteStatusesAsync(TEntity entity)
        {
            List<ExecutionStatus> remoteCommandsStatuses = [];
            if (_nodeMetadata.NodeType == NodeType.Master)
            {
                var fullyQualifiedName = _entityDiscoveryService.Discover(record => record.IterationId == entity.Id).Single().FullyQualifiedName;
                if (_nodeMetadata.NodeType == NodeType.Master)
                {
                    foreach (var node in _nodeRegistry.Query(node => node.Metadata.NodeType == NodeType.Worker && (node.NodeStatus == NodeStatus.Running|| node.NodeStatus == NodeStatus.Pending)))
                    {
                        var client = _grpcClientFactory.GetClient<GrpcMonitorClient>(node.Metadata.NodeIP);
                        remoteCommandsStatuses = await client.QueryStatusesAsync(fullyQualifiedName);
                    }
                }
            }
            return remoteCommandsStatuses;
        }
    }

}
