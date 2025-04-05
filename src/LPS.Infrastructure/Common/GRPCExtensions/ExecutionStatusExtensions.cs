using DomainStatus = LPS.Domain.Domain.Common.Enums.ExecutionStatus;
using GrpcStatus = LPS.Protos.Shared.ExecutionStatus;

namespace LPS.Infrastructure.Common.GRPCExtensions
{
    public static class ExecutionStatusExtensions
    {
        /// <summary>
        /// Maps a domain enum value to its corresponding gRPC enum.
        /// </summary>
        public static GrpcStatus ToGrpc(this DomainStatus status)
        {
            return status switch
            {
                DomainStatus.PendingExecution => GrpcStatus.PendingExecution,
                DomainStatus.Scheduled => GrpcStatus.Scheduled,
                DomainStatus.Ongoing => GrpcStatus.Ongoing,
                DomainStatus.Completed => GrpcStatus.Completed,
                DomainStatus.Paused => GrpcStatus.Paused,
                DomainStatus.Cancelled => GrpcStatus.Cancelled,
                DomainStatus.Failed => GrpcStatus.Failed,
                DomainStatus.Unkown => GrpcStatus.Unkown,
                _ => GrpcStatus.Unkown
            };
        }

        /// <summary>
        /// Maps a gRPC enum value to its corresponding domain enum.
        /// </summary>
        public static DomainStatus ToLocal(this GrpcStatus status)
        {
            return status switch
            {
                GrpcStatus.PendingExecution => DomainStatus.PendingExecution,
                GrpcStatus.Scheduled => DomainStatus.Scheduled,
                GrpcStatus.Ongoing => DomainStatus.Ongoing,
                GrpcStatus.Completed => DomainStatus.Completed,
                GrpcStatus.Paused => DomainStatus.Paused,
                GrpcStatus.Cancelled => DomainStatus.Cancelled,
                GrpcStatus.Failed => DomainStatus.Failed,
                GrpcStatus.Unkown => DomainStatus.Unkown,
                _ => DomainStatus.Unkown
            };
        }

    }
}
