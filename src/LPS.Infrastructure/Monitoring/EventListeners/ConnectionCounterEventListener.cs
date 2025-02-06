using System.Diagnostics.Tracing;
using System;
using System.Collections.Concurrent; // Add this for ConcurrentDictionary
using System.Collections.Generic;
using System.Linq;

public class ConnectionCounterEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        try
        {
            if (!eventSource.Name.Equals("lps.active.connections"))
            {
                return;
            }
            var args = new Dictionary<string, string?> { ["EventCounterIntervalSec"] = "1" };
            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All, args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
        }
    }

    // Change type to ConcurrentDictionary
    private static ConcurrentDictionary<string, int> _hostActiveConnectionsCount = new ConcurrentDictionary<string, int>();

    public int GetHostActiveConnectionsCount(string hostName)
    {
        // Use GetOrAdd method for thread-safe access
        return _hostActiveConnectionsCount.GetOrAdd(hostName, 0);
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        try
        {
            string hostName = string.Empty;
            int activeConnectionCount = -1;
            if (eventData.EventId == 1 || eventData.EventId == 2)
            {
                hostName = (string)eventData.Payload[0];
                activeConnectionCount = (int)eventData.Payload[1];
            }

            if (!string.IsNullOrEmpty(hostName) && activeConnectionCount >= 0)
            {
                // Use AddOrUpdate method for thread-safe update or add
                _hostActiveConnectionsCount.AddOrUpdate(hostName, activeConnectionCount, (key, existingVal) => activeConnectionCount);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
        }
    }
}
