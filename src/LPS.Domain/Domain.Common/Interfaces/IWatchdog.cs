using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.Common.Interfaces
{
    public enum ResourceState { 
        Cool,
        Cooling,
        Hot,
        Unknown
    }
    public interface IWatchdog
    {
        /// <summary>
        /// Balances resource usage by initiating cooling if necessary.
        /// </summary>
        /// <param name="hostName">The host name to monitor.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The current resource state after balancing.</returns>
        public Task<ResourceState> BalanceAsync(string hostName, CancellationToken token = default);
    }
}
