#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.LPSClients.SessionManager;

namespace LPS.Infrastructure.LPSClients.GlobalVariableManager
{
    public partial class VariableManager(IRuntimeOperationIdProvider operationProvider, ILogger logger) : IVariableManager
    {
        private readonly ConcurrentDictionary<string, IVariableHolder> _variables = new();
        private readonly IRuntimeOperationIdProvider _operationIdProvider= operationProvider;
        private readonly ILogger _logger= logger;
        public async Task AddVariableAsync(string variableName, IVariableHolder variableHolder, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(variableName))
                throw new ArgumentException("Variable name cannot be null or whitespace.", nameof(variableName));

            if (variableHolder == null)
                throw new ArgumentNullException(nameof(variableHolder), "Variable holder cannot be null.");

            if (!_variables.TryAdd(variableName, variableHolder))
            {
                await _logger.LogAsync(_operationIdProvider.OperationId, $"Variable '{{variableName}}' already exists and will be overridden", LPSLoggingLevel.Warning, token);
                // Override the existing variable
                _variables[variableName] = variableHolder;
            }
        }

        public async Task<IVariableHolder?> GetVariableAsync(string variableName, CancellationToken token)
        {
            if (_variables.TryGetValue(variableName.Trim(), out var variableHolder))
            {
                return variableHolder;
            }
            await _logger.LogAsync(_operationIdProvider.OperationId, $"Variable ${variableName} does not exist", LPSLoggingLevel.Warning, token);
            return null;
        }
        public async Task RemoveVariableAsync(string variableName, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(variableName))
                throw new ArgumentException("Variable name cannot be null or whitespace.", nameof(variableName));

            if (_variables.TryRemove(variableName, out var removedVariable))
            {
                await _logger.LogAsync(_operationIdProvider.OperationId, $"Variable '{variableName}' was successfully removed.", LPSLoggingLevel.Information, token);
            }
            else
            {
                await _logger.LogAsync(_operationIdProvider.OperationId, $"Attempted to remove non-existent variable '{variableName}'.", LPSLoggingLevel.Warning, token);
            }
        }

    }
}
