using LPS.Infrastructure.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Common.Options
{
    public class ClusterConfigurationOptions
    {
        public string? MasterNodeIP { get; set; }
        public int? GRPCPort { get; set; }
        public int? ExpectedNumberOfWorkers { get; set; }
        public bool? MasterNodeIsWorker { get; set; }
    }
}
