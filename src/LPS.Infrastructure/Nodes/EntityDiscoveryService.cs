#nullable enable

using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Nodes
{
    public class EntityDiscoveryService: IEntityDiscoveryService
    {
        ILogger _logger;
        IRuntimeOperationIdProvider _operationIdProvider;
        public EntityDiscoveryService(ILogger logger, IRuntimeOperationIdProvider operationIdProvider = null)
        {
            _entityDiscoveryRecords = new List<EntityDiscoveryRecord>();
            _logger = logger;
            _operationIdProvider = operationIdProvider;
        }

        private readonly ICollection<EntityDiscoveryRecord> _entityDiscoveryRecords;

        public void AddEntityDiscoveryRecord(string fullyQualifiedName, Guid roundId, Guid iterationId, Guid requestId, INode node)
        {
            var record = new EntityDiscoveryRecord(fullyQualifiedName, roundId, iterationId, requestId, node);

            if (!_entityDiscoveryRecords.Contains(record))
            {
                _logger.Log(_operationIdProvider.OperationId, $"entity with FQDN '{fullyQualifiedName}' and request Id '{requestId}' has been added to discovery record", LPSLoggingLevel.Verbose);
                _entityDiscoveryRecords.Add(record);
            }
            else {
                _logger.Log(_operationIdProvider.OperationId, $"entity with FQDN '{fullyQualifiedName}' and request Id '{requestId}' already exists", LPSLoggingLevel.Verbose);
            }
        }
        public ICollection<EntityDiscoveryRecord>? Discover(Func<EntityDiscoveryRecord, bool> predict)
        {
            return _entityDiscoveryRecords.Where(predict).ToList();
        }
    }

    public record EntityDiscoveryRecord
    {
        public EntityDiscoveryRecord(string fullyQualifiedName, Guid roundId, Guid iterationId, Guid requestId, INode node)
        {
            FullyQualifiedName = fullyQualifiedName;
            RoundId = roundId;
            IterationId = iterationId;
            RequestId = requestId;
            Node = node;
        }
        public string FullyQualifiedName { get; }
        public Guid RoundId { get; }
        public Guid IterationId { get; }
        public Guid RequestId { get; }
        public INode Node { get; }
    }
}
