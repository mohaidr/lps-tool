using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using LPS.Protos.Shared;
using LPS.Infrastructure.Common.GRPCExtensions;

namespace LPS.Infrastructure.Nodes
{

    public class Node : INode
    {
        IClusterConfiguration _clusterConfiguration;
        INodeRegistry _nodeRegistry;
        public Node(INodeMetadata metadata, IClusterConfiguration clusterConfiguration, INodeRegistry nodeRegistry)
        {
            Metadata = metadata;
            NodeStatus = NodeStatus.Pending;
            _nodeRegistry = nodeRegistry;
            _clusterConfiguration = clusterConfiguration;
        }

        public INodeMetadata Metadata { get; }

        public NodeStatus NodeStatus { get; protected set; }

        public async ValueTask<SetNodeStatusResponse> SetNodeStatus(NodeStatus nodeStatus)
        {
            NodeStatus = nodeStatus;
            var localNode = _nodeRegistry.GetLocalNode();
            if (localNode.Metadata.NodeType != NodeType.Master)
            {
                var channel = GrpcChannel.ForAddress($"http://{_clusterConfiguration.MasterNodeIP}:{_clusterConfiguration.GRPCPort}");

                // Create the gRPC Client
                var client = new NodeService.NodeServiceClient(channel);

                var response = await client.SetNodeStatusAsync(new SetNodeStatusRequest() { NodeIp = this.Metadata.NodeIP, NodeName = this.Metadata.NodeName, Status = nodeStatus.ToGrpc() });

                return response;

            }
            return new SetNodeStatusResponse() { Success = true, Message = "Master Node Status has been updated" };
        }
    }
}
