using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Nodes
{
    public interface INodeRegistry
    {
        public void RegisterNode(INode node);
        public void UnregisterNode(INode node);
        public IEnumerable<INode> Query(Func<INode, bool> predicate);
        public INode GetMasterNode();
        public INode GetLocalNode();
        public IEnumerable<INode> GetNeighborNodes();
    }
}
