using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LPS.Protos.Shared;

namespace LPS.Infrastructure.Nodes
{
    public enum NodeType
    {
        Master,
        Worker
    }
    public enum NodeStatus
    {
        Pending,
        Ready,
        Running,
        Failed,
        Stopped
    }
    public interface INode
    {
        public static string NodeIP => GetLocalIPAddress();
        static string GetLocalIPAddress()
        {
            return Dns.GetHostAddresses(Dns.GetHostName())
                      .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                      .ToString() ?? "No IPv4 Address Found";
        }
        INodeMetadata Metadata { get; }

        NodeStatus NodeStatus { get; }
        public ValueTask<SetNodeStatusResponse> SetNodeStatus(NodeStatus nodeStatus);

    }
}
