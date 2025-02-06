#nullable enable

using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace LPS.Infrastructure.LPSClients.SessionManager
{
    public class SessionManager(IRuntimeOperationIdProvider operationProvider, ILogger logger) : ISessionManager
    {
        private readonly ConcurrentDictionary<string, IClientSession> _sessions = new();
        private readonly IRuntimeOperationIdProvider _operationIdProvider = operationProvider;
        private readonly ILogger _logger = logger;

        public async Task AddResponseAsync(string sessionId, string variableName, IVariableHolder capturedResponse, CancellationToken token)
        {
            var session = _sessions.GetOrAdd(sessionId, _ => new ClientSession(sessionId, _operationIdProvider, _logger));
            await session.AddResponseAsync(variableName, capturedResponse, token);
        }

        public async Task<IVariableHolder?> GetResponseAsync(string sessionId, string variableName, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(sessionId) && _sessions.TryGetValue(sessionId, out var session))
                return session.GetResponse(variableName);

            await _logger.LogAsync(_operationIdProvider.OperationId, $"No Session variable named '${variableName}' is defined for the session with Id: {sessionId}", LPSLoggingLevel.Warning, token);
            return null;
        }

        public void CleanupSession(string clientId)
        {
            _sessions.TryRemove(clientId, out _);
        }
    }
}
