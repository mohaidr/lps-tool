using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Nodes
{
    public interface IClusterConfiguration
    {
        string MasterNodeIP {  get;}
        public int GRPCPort { get; }
        public int ExpectedNumberOfWorkers { get;}

        public bool MasterNodeIsWorker { get; }
    }
}
