using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Nodes
{
    public class ClusterConfiguration : IClusterConfiguration
    {
        public string MasterNodeIP { get; }
        public int GRPCPort { get;}
        public int ExpectedNumberOfWorkers { get;}
        public bool MasterNodeIsWorker { get; }

        private ClusterConfiguration(string masterNodeIp, int defaultGrpcPort)
        {
            MasterNodeIP = masterNodeIp;
            GRPCPort = defaultGrpcPort;
            ExpectedNumberOfWorkers = 1;
            MasterNodeIsWorker = true;
        }

        public ClusterConfiguration(string masterNodeIP, int gRPCPort, bool masterIsWorker, int expectedNumberOfWorkers)
        {
            MasterNodeIP = masterNodeIP;
            GRPCPort = gRPCPort;
            ExpectedNumberOfWorkers = expectedNumberOfWorkers;
            MasterNodeIsWorker = masterIsWorker;
        }

        public static ClusterConfiguration GetDefaultInstance(string masterNodeIp, int defaultGrpcPort)
        {
            return new ClusterConfiguration(masterNodeIp, defaultGrpcPort);
        }
    }
}
