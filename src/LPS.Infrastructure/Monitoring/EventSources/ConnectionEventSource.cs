using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

[EventSource(Name = "lps.active.connections")]
internal class ConnectionEventSource : EventSource
{
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    private static readonly Lazy<ConnectionEventSource> lazyInstance = new Lazy<ConnectionEventSource>(() => new ConnectionEventSource());
    internal static ConnectionEventSource Log => lazyInstance.Value;

    // Private constructor to prevent external instantiation
    private ConnectionEventSource()
    {
    }

    // Track the active connection count
    private static Dictionary<string, int> _activeConnectionsCount = new Dictionary<string, int>();
    private static Dictionary<string, int> _numberOfSentRequests = new Dictionary<string, int>();
    private static Dictionary<string, int> _numberOfRequestsPerSecond = new Dictionary<string, int>();

    [Event(1, Message = "Connection established: {0}, Active connection count: {1}")]
    internal async Task  ConnectionEstablished(string hostName, int numberOfActiveConnections = -1)
    {
        if (IsEnabled())
        {
           await semaphore.WaitAsync();
            if (!_activeConnectionsCount.ContainsKey(hostName))
                _activeConnectionsCount[hostName] = 0;
            try
            {
                int currentCount = _activeConnectionsCount[hostName];
                _activeConnectionsCount[hostName] = numberOfActiveConnections != -1 ? numberOfActiveConnections : Interlocked.Increment(ref currentCount);
                WriteEvent(1, hostName, currentCount);
            }
            finally
            {
                semaphore.Release();
            }
        }

    }

    [Event(2, Message = "Connection closed: {0}, Active connection count: {1}")]
    internal async Task ConnectionClosed(string hostName, int numberOfActiveConnections = -1)
    {
        if (IsEnabled())
        {
            await semaphore.WaitAsync();
            if (!_activeConnectionsCount.ContainsKey(hostName))
                return;
            try
            {
                int currentCount = _activeConnectionsCount[hostName];
                _activeConnectionsCount[hostName] = numberOfActiveConnections != -1 ? numberOfActiveConnections : Interlocked.Decrement(ref currentCount);
                WriteEvent(2, hostName, currentCount);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
