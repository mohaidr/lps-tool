using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.GRPCClients.Factory
{
    public interface ISelfGRPCClient
    {
       public ISelfGRPCClient GetClient (string grpcAddress);
    }

}
