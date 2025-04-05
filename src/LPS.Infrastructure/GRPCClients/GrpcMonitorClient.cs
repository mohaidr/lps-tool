using Grpc.Net.Client;
using LPS.Domain.Domain.Common.Enums;
using LPS.Infrastructure.Common.GRPCExtensions;
using LPS.Protos.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using LPS.Infrastructure.GRPCClients.Factory;
using System.Net.Sockets;
using System;
using Grpc.Core;

namespace LPS.Infrastructure.GRPCClients
{
    public class GrpcMonitorClient : IGRPCClient, ISelfGRPCClient
    {
        private readonly MonitorService.MonitorServiceClient _client;

        public GrpcMonitorClient() 
        {
        }
        private GrpcMonitorClient(string grpcAddress)
        {
            var channel = Grpc.Net.Client.GrpcChannel.ForAddress(grpcAddress);
            _client = new MonitorService.MonitorServiceClient(channel);
        }

        public async Task<List<Domain.Domain.Common.Enums.ExecutionStatus>> QueryStatusesAsync(string fqdn, CancellationToken token = default)
        {

            var request = new StatusQueryRequest { FullyQualifiedName = fqdn };
            var response = await _client.QueryIterationStatusesAsync(request, cancellationToken: token);
            return response.Statuses.Select(s => s.ToLocal()).ToList();
        }

        public async Task<bool> MonitorAsync(string fqdn, CancellationToken token = default)
        {
            try
            {
                var request = new MonitorRequest { FullyQualifiedName = fqdn };
                var response = await _client.MonitorAsync(request, cancellationToken: token);

                if (!response.Success)
                {
                    throw new InvalidOperationException($"Monitor call failed: {response.Message}");
                }

                return true;
            }
            catch (RpcException rpcEx)
            {
                // Optional: handle gRPC-specific errors
                throw new InvalidOperationException($"gRPC Monitor call failed: {rpcEx.Status.Detail}", rpcEx);
            }
        }

        public ISelfGRPCClient GetClient(string grpcAddress)
        {
           return new GrpcMonitorClient(grpcAddress);
        }
    }
}
