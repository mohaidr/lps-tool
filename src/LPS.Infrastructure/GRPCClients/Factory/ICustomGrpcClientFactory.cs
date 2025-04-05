using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.GRPCClients.Factory
{
    public interface ICustomGrpcClientFactory
    {
        TClient GetClient<TClient>(string grpcAddress) where TClient :  ISelfGRPCClient, new();
    }

}
