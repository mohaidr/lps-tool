using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Nodes
{
    public interface INodeMetadata
    {
        string NodeName { get; }
        public string NodeIP { get; }
        NodeType NodeType { get; }
        string OS { get; }
        string Architecture { get; }
        string Framework { get; }
        string CPU { get; }
        int LogicalProcessors { get; }
        string TotalRAM { get; }
        List<IDiskInfo> Disks { get; }
        List<INetworkInfo> NetworkInterfaces { get; }
    }

    public interface IDiskInfo
    {
        string Name { get; }
        string TotalSize { get; }
        string FreeSpace { get; }
    }

    public interface INetworkInfo
    {
        string InterfaceName { get; }
        string Type { get; }
        string Status { get; }
        List<string> IpAddresses { get; }
    }
}
