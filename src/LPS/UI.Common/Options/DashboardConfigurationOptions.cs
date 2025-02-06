using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Common.Options
{
    public class DashboardConfigurationOptions
    {
        public bool? BuiltInDashboard { get; set; }
        public int? Port { get; set; }
        public int? RefreshRate { get; set; }
    }
}
