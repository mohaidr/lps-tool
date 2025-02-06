#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.SessionManager
{
    public interface IClientSession
    {
        public Task AddResponseAsync(string variableName, IVariableHolder response, CancellationToken token);
        public IVariableHolder? GetResponse(string variableName);
    }
}
