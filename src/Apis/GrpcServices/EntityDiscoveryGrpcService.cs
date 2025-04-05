using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Tasks;
using LPS.Infrastructure.Nodes;
using LPS.Protos.Shared;
using Node = LPS.Protos.Shared.Node;

namespace LPS.GrpcServices
{
    public class EntityDiscoveryGrpcService : EntityDiscoveryProtoService.EntityDiscoveryProtoServiceBase
    {
        private readonly IEntityDiscoveryService _entityDiscoveryService;
        private readonly INodeRegistry _nodeRegistry;
        public EntityDiscoveryGrpcService(IEntityDiscoveryService entityDiscoveryService, INodeRegistry nodeRegistry)
        {
            _entityDiscoveryService = entityDiscoveryService;
            _nodeRegistry = nodeRegistry;
        }

        public override Task<Empty> AddEntityDiscoveryRecord(Protos.Shared.EntityDiscoveryRecord record, ServerCallContext context)
        {
            var node = _nodeRegistry.Query(n => n.Metadata.NodeName == record.Node.Name && n.Metadata.NodeIP == record.Node.NodeIP).FirstOrDefault();
            if (node == null) {
                throw new RpcException(new Status(StatusCode.NotFound, "Node does not exist"));
            }
            if (string.IsNullOrWhiteSpace(record.FullyQualifiedName) ||
                string.IsNullOrWhiteSpace(record.RoundId) ||
                string.IsNullOrWhiteSpace(record.IterationId) ||
                string.IsNullOrWhiteSpace(record.RequestId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "FullyQualifiedName and all Ids (RoundId, IterationId, RequestId) must be provided."));
            }

            _entityDiscoveryService.AddEntityDiscoveryRecord(
                record.FullyQualifiedName,
                Guid.Parse(record.RoundId),
                Guid.Parse(record.IterationId),
                Guid.Parse(record.RequestId),node);
            return Task.FromResult(new Empty());
        }

        public override Task<EntityDiscoveryRecordResponse> DiscoverEntity(EntityDiscoveryQuery query, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(query.FullyQualifiedName))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "FullyQualifiedName must be provided."));
            }
            var record = _entityDiscoveryService.Discover(r=> r.FullyQualifiedName == query.FullyQualifiedName)?.FirstOrDefault();

            if (record == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Entity not found"));
            }

            return Task.FromResult(new EntityDiscoveryRecordResponse
            {
                FullyQualifiedName = record.FullyQualifiedName,
                RoundId = record.RoundId.ToString(),
                IterationId = record.IterationId.ToString(),
                RequestId = record.RequestId.ToString(),
                Node = new Node
                {
                    Name = record.Node.Metadata.NodeName,
                    NodeIP = record.Node.Metadata.NodeIP
                }
            });
        }
    }
}
