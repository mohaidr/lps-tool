using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LPS.Infrastructure.Nodes
{
    public record NodeMetadata : INodeMetadata
    {
        public string NodeName { get; }
        public string NodeIP { get; }
        public NodeType NodeType { get; private set; }
        public string OS { get; }
        public string Architecture { get; }
        public string Framework { get; }
        public string CPU { get; }
        public int LogicalProcessors { get; }
        public string TotalRAM { get; }
        public List<IDiskInfo> Disks { get; }
        public List<INetworkInfo> NetworkInterfaces { get; }

        public NodeMetadata(IClusterConfiguration clusterConfiguration)
        {
            ArgumentNullException.ThrowIfNull(clusterConfiguration);
            NodeType = INode.NodeIP == clusterConfiguration.MasterNodeIP ? NodeType.Master : NodeType.Worker;
            NodeName = Environment.MachineName;
            NodeIP = INode.NodeIP;
            OS = RuntimeInformation.OSDescription;
            Architecture = RuntimeInformation.OSArchitecture.ToString();
            Framework = RuntimeInformation.FrameworkDescription;
            CPU = GetCpuInfo();
            LogicalProcessors = Environment.ProcessorCount;
            TotalRAM = GetMemoryInfo();
            Disks = GetDiskInfo();
            NetworkInterfaces = GetNetworkInfo();
        }

        public NodeMetadata(
            IClusterConfiguration clusterConfiguration,
            string nodeName,
            string nodeIP,
            string os,
            string architecture,
            string framework,
            string cpu,
            int logicalProcessors,
            string totalRam,
            List<IDiskInfo> disks,
            List<INetworkInfo> networkInterfaces)
        {
            ArgumentNullException.ThrowIfNull(clusterConfiguration);
            NodeType = nodeIP == clusterConfiguration.MasterNodeIP ? NodeType.Master : NodeType.Worker;
            NodeName = nodeName;
            NodeIP = nodeIP;
            OS = os;
            Architecture = architecture;
            Framework = framework;
            CPU = cpu;
            LogicalProcessors = logicalProcessors;
            TotalRAM = totalRam;
            Disks = disks ?? throw new ArgumentNullException(nameof(disks));
            NetworkInterfaces = networkInterfaces ?? throw new ArgumentNullException(nameof(networkInterfaces));
        }

        private static string GetCpuInfo()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Unknown CPU" :
                File.ReadLines("/proc/cpuinfo").FirstOrDefault(line => line.StartsWith("model name"))?.Split(":")[1].Trim() ?? "Unknown CPU";
        }

        private static string GetMemoryInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "Unknown RAM";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return File.ReadLines("/proc/meminfo").FirstOrDefault(line => line.StartsWith("MemTotal"))?.Split(":")[1].Trim() ?? "Unknown RAM";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var output = "Unknown RAM";
                return output;
            }
            return "Unknown RAM";
        }

        private static List<IDiskInfo> GetDiskInfo()
        {
            var disks = new List<IDiskInfo>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                {
                    disks.Add(new DiskInfo(drive.Name, drive.TotalSize.ToString(), drive.AvailableFreeSpace.ToString()));
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var output = File.ReadAllLines("/proc/partitions")
                    .Skip(2) // Skip headers
                    .Select(line => line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    .Where(parts => parts.Length == 4)
                    .Select(parts => new DiskInfo(parts[3], "Unknown Size", "Unknown Free Space"))
                    .ToList();

                disks.AddRange(output);
            }

            return disks;
        }
        private static List<INetworkInfo> GetNetworkInfo()
        {
            var networkInterfaces = new List<INetworkInfo>();

            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                var addresses = netInterface.GetIPProperties().UnicastAddresses
                    .Select(ip => ip.Address.ToString())
                    .ToList();

                networkInterfaces.Add(new NetworkInfo(
                    netInterface.Name,
                    netInterface.NetworkInterfaceType.ToString(),
                    netInterface.OperationalStatus.ToString(),
                    addresses));
            }

            return networkInterfaces;
        }

    }

    public record DiskInfo : IDiskInfo
    {
        public string Name { get; }
        public string TotalSize { get; }
        public string FreeSpace { get; }

        public DiskInfo(string name, string totalSize, string freeSpace)
        {
            Name = name;
            TotalSize = totalSize;
            FreeSpace = freeSpace;
        }
    }

    public record NetworkInfo : INetworkInfo
    {
        public string InterfaceName { get; }
        public string Type { get; }
        public string Status { get; }
        public List<string> IpAddresses { get; }

        public NetworkInfo(string interfaceName, string type, string status, List<string> ipAddresses)
        {
            InterfaceName = interfaceName;
            Type = type;
            Status = status;
            IpAddresses = ipAddresses ?? new List<string>();
        }
    }
}
