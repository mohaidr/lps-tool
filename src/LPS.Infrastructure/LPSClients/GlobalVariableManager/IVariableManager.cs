#nullable enable
using LPS.Infrastructure.LPSClients.SessionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.GlobalVariableManager
{
    public interface IVariableManager
    {
        Task AddVariableAsync(string variableName, IVariableHolder variableHolder, CancellationToken token);
        Task<IVariableHolder?> GetVariableAsync(string variableName, CancellationToken token);
        Task RemoveVariableAsync(string variableName, CancellationToken token = default);
    }

}
