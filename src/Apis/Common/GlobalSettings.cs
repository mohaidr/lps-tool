using System;
using System.Net;
using System.Net.Sockets;

namespace Apis.Common
{
    public static class GlobalSettings
    {
        private static readonly int _port;

        // Static constructor to initialize the read-only port
        static GlobalSettings()
        {
            _port = GenerateRandomPort();
        }

        public static int DefaultDashboardPort => 59444;
        public static int DefaultGRPCPort => 5001;

        private static int GenerateRandomPort()
        {
            // Define the range for ephemeral ports.
            const int minPort = 49152;
            const int maxPort = 65535;

            int port;
            do
            {
                // Generate a random port number within the range.
                port = new Random().Next(minPort, maxPort + 1);
            }
            while (!IsPortAvailable(port));

            return port;
        }

        private static bool IsPortAvailable(int port)
        {
            // Check if the port is available for use
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
