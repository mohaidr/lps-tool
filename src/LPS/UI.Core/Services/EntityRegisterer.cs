using Grpc.Net.Client;
using LPS.Domain;
using LPS.Infrastructure.Nodes;
using LPS.Protos.Shared;

namespace LPS.UI.Core.Services
{
    internal class EntityRegisterer
    {
        IClusterConfiguration _clusterConfiguration;
        IEntityDiscoveryService _entityDiscoveryService;
        private readonly INodeMetadata _nodeMetaData;
        INodeRegistry _nodeRegistry;
        public EntityRegisterer(IClusterConfiguration clusterConfiguration,
            INodeMetadata nodeMetaData, 
            IEntityDiscoveryService entityDiscoveryService, 
            INodeRegistry nodeRegistry) 
        { 
            _clusterConfiguration = clusterConfiguration;
            _nodeMetaData = nodeMetaData;
            _nodeRegistry = nodeRegistry;
            _entityDiscoveryService = entityDiscoveryService;
        }

        public void RegisterEntities(Plan plan)
        {
            var channel = GrpcChannel.ForAddress($"http://{_clusterConfiguration.MasterNodeIP}:{_clusterConfiguration.GRPCPort}");
            var grpcClient = new EntityDiscoveryProtoService.EntityDiscoveryProtoServiceClient(channel);
            foreach (var round in plan.GetReadOnlyRounds())
            {
                foreach (var iteration in round.GetReadOnlyIterations())
                {
                    if (((HttpIteration)iteration).HttpRequest != null)
                    {
                        var entityName = $"plan/{plan.Name}/round/{round.Name}/Iteration/{iteration.Name}";
                        _entityDiscoveryService.AddEntityDiscoveryRecord(entityName, round.Id, iteration.Id, ((HttpIteration)iteration).HttpRequest.Id, _nodeRegistry.GetLocalNode()); // register locally
                        if (_nodeMetaData.NodeType != Infrastructure.Nodes.NodeType.Master)
                        {
                            var request = new Protos.Shared.EntityDiscoveryRecord
                            {
                                FullyQualifiedName = entityName,
                                RoundId = round.Id.ToString(),
                                IterationId = iteration.Id.ToString(),
                                RequestId = ((HttpIteration)iteration).HttpRequest.Id.ToString(),
                                Node = new Protos.Shared.Node
                                {
                                    Name = _nodeMetaData.NodeName,
                                    NodeIP = _nodeMetaData.NodeIP,
                                }
                            };

                            grpcClient.AddEntityDiscoveryRecord(request);// register on the master
                        }
                    }
                }
            }
        }
    }
}
