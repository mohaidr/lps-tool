#nullable enable

using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.SessionManager
{
    public class ClientSession(string sessionId, IRuntimeOperationIdProvider operationProvider, ILogger logger) : IClientSession
    {
        public string SessionId { get; } = sessionId;
        private readonly ConcurrentDictionary<string, IVariableHolder> _variables = new();
        private readonly IRuntimeOperationIdProvider _operationIdProvider = operationProvider;
        private readonly ILogger _logger = logger;

        public async Task  AddResponseAsync(string variableName, IVariableHolder variableHolder, CancellationToken token = default)
        {
            if (!_variables.TryAdd(variableName, variableHolder))
            {
                await _logger.LogAsync(_operationIdProvider.OperationId, $" Variable '{{variableName}}' already exists and will be overridden", LPSLoggingLevel.Warning, token);
                // Override the existing variable
                _variables[variableName] = variableHolder;
            }
        }

        public IVariableHolder? GetResponse(string variableName)
        {
            return _variables.TryGetValue(variableName, out var response) ? response : null;
        }
    }
}
