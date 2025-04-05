using LPS.Domain.Domain.Common.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.GRPCClients
{
    /// <summary>
    /// Provides access to remote gRPC services for querying command execution statuses.
    /// </summary>
    public interface IGrpcStatusClient
    {
        /// <summary>
        /// Queries the command execution statuses of a specific entity using its Fully Qualified Domain Name (FQDN)
        /// from a remote gRPC endpoint.
        /// </summary>
        /// <param name="fqdn">Fully Qualified Domain Name of the iteration.</param>
        /// <param name="grpcAddress">The gRPC base address of the remote node (e.g., http://localhost:5000).</param>
        /// <param name="token">Optional cancellation token.</param>
        /// <returns>A list of execution statuses for the given FQDN from the remote node.</returns>
        Task<List<ExecutionStatus>> QueryStatusesAsync(string fqdn, string grpcAddress, CancellationToken token = default);
    }
}
