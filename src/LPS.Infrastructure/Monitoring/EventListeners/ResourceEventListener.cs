using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace LPS.Infrastructure.Monitoring.EventListeners
{
    internal class ResourceEventListener : EventListener
    {
        private double _memoryUsageMB;
        private double _cpuTime;

        public ResourceEventListener()
        {
        }

        public double MemoryUsageMB => _memoryUsageMB;
        public double CPUPercentage => _cpuTime;

        protected override void OnEventSourceCreated(EventSource source)
        {
            if (source.Name.Equals("System.Runtime"))
            {
                EnableEvents(source, EventLevel.Verbose, EventKeywords.All, new Dictionary<string, string>()
                {
                    ["EventCounterIntervalSec"] = "1"
                });
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.Payload == null || !eventData.EventName.Equals("EventCounters"))
            {
                return;
            }

            for (int i = 0; i < eventData.Payload.Count; i++)
            {
                if (eventData.Payload[i] is IDictionary<string, object> eventPayload)
                {
                    var (counterName, counterValue) = GetRelevantMetric(eventPayload);
                    switch (counterName)
                    {
                        case "working-set":
                            // Get private memory size (which includes virtual memory as well)
                            _memoryUsageMB = GetPrivateMemoryUsageMB();
                            break;
                        case "cpu-usage":
                            double.TryParse(counterValue, out _cpuTime);
                            break;
                    }
                }
            }
        }

        private static (string counterName, string counterValue) GetRelevantMetric(
            IDictionary<string, object> eventPayload)
        {
            var counterName = string.Empty;
            var counterValue = string.Empty;

            if (eventPayload.TryGetValue("Name", out object displayValue))
            {
                counterName = displayValue.ToString();
            }

            if (eventPayload.TryGetValue("Mean", out object value) ||
                eventPayload.TryGetValue("Increment", out value))
            {
                counterValue = value.ToString();
            }

            return (counterName, counterValue);
        }

        // New method to get the private memory usage in MB from the process
        private double GetPrivateMemoryUsageMB()
        {
            using (var process = Process.GetCurrentProcess())
            {
                // PrivateMemorySize64 gives the total private memory size in bytes
                return process.PrivateMemorySize64 / (1024.0 * 1024.0); // Convert to MB
            }
        }
    }
}
