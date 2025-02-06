using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Watchdog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Common.Options
{
    public class WatchdogOptions
    {
        public int? MaxMemoryMB { get; set; }
        public int? CoolDownMemoryMB { get; set; }
        public int? MaxCPUPercentage { get; set; }
        public int? CoolDownCPUPercentage { get; set; }
        public int? CoolDownRetryTimeInSeconds { get; set; }
        public int? MaxConcurrentConnectionsCountPerHostName { get; set; }
        public int? CoolDownConcurrentConnectionsCountPerHostName { get; set; }
        public int? MaxCoolingPeriod { get; set; }
        public int? ResumeCoolingAfter { get; set; }
        public SuspensionMode? SuspensionMode { get; set; }
    }
}
