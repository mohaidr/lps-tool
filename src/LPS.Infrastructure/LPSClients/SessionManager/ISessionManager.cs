#nullable enable

using LPS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.SessionManager
{
    public interface ISessionManager
    {
        public Task AddResponseAsync(string sessionId, string variableName, IVariableHolder response, CancellationToken token);
        public Task<IVariableHolder?> GetResponseAsync(string sessionId, string variableName, CancellationToken token);
        public void CleanupSession(string sessionId);
    }
}
