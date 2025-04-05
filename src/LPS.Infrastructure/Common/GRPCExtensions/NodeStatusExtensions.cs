using System;
using GrpcNodeStatus = LPS.Protos.Shared.NodeStatus;
using LocalNodeStatus = LPS.Infrastructure.Nodes.NodeStatus;

namespace LPS.Infrastructure.Common.GRPCExtensions
{
    public static class NodeStatusExtensions
    {
        /// <summary>
        /// Maps the gRPC NodeStatus enum to the internal domain NodeStatus enum.
        /// </summary>
        public static LocalNodeStatus ToLocal(this GrpcNodeStatus protoStatus)
        {
            return protoStatus switch
            {
                GrpcNodeStatus.Running => LocalNodeStatus.Running,
                GrpcNodeStatus.Ready => LocalNodeStatus.Ready,
                GrpcNodeStatus.Stopped => LocalNodeStatus.Stopped,
                GrpcNodeStatus.Failed => LocalNodeStatus.Failed,
                GrpcNodeStatus.Pending => LocalNodeStatus.Pending,
                _ => throw new NotImplementedException($"Unhandled proto NodeStatus: {protoStatus}")
            };
        }

        /// <summary>
        /// Maps the internal domain NodeStatus enum to the gRPC NodeStatus enum.
        /// </summary>
        public static GrpcNodeStatus ToGrpc(this LocalNodeStatus internalStatus)
        {
            return internalStatus switch
            {
                LocalNodeStatus.Running => GrpcNodeStatus.Running,
                LocalNodeStatus.Ready => GrpcNodeStatus.Ready,
                LocalNodeStatus.Stopped => GrpcNodeStatus.Stopped,
                LocalNodeStatus.Failed => GrpcNodeStatus.Failed,
                LocalNodeStatus.Pending => GrpcNodeStatus.Pending,
                _ => throw new NotImplementedException($"Unhandled internal NodeStatus: {internalStatus}")
            };
        }
    }
}
