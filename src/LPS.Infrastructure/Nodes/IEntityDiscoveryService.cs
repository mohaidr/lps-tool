#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Nodes
{
    public interface IEntityDiscoveryService
    {
        void AddEntityDiscoveryRecord(string fullyQualifiedName, Guid roundId, Guid iterationId, Guid requestId, INode node);
        ICollection<EntityDiscoveryRecord>? Discover(Func<EntityDiscoveryRecord, bool> predict);
    }

}
