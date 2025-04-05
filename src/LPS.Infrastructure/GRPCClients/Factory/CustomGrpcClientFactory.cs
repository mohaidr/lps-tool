using LPS.Infrastructure.Nodes;
using System;
using System.Collections.Concurrent;

namespace LPS.Infrastructure.GRPCClients.Factory
{


    public class CustomGrpcClientFactory : ICustomGrpcClientFactory
    {
        private readonly ConcurrentDictionary<(Type, string), ISelfGRPCClient> _instances = new();
        IClusterConfiguration _clusterConfiguration;
        public CustomGrpcClientFactory(IClusterConfiguration clusterConfiguration) { 
            _clusterConfiguration = clusterConfiguration;
        }
        public TClient GetClient<TClient>(string grpcAddress) where TClient :  ISelfGRPCClient, new()
        {
            var key = (typeof(TClient), grpcAddress);

            if (_instances.TryGetValue(key, out var existing))
                return (TClient)existing;

            // Register on first use (activates using constructor with string)
            var instance = CreateClient<TClient>(grpcAddress);
            _instances[key] = instance;

            return instance;
        }

        private TClient CreateClient<TClient>(string grpcAddress) where TClient:  ISelfGRPCClient, new()
        {
            return (TClient)new TClient().GetClient($"http://{grpcAddress}:{_clusterConfiguration.GRPCPort}");
        }
    }

}
