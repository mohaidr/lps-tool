using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRun.IterationMode
{

    /// <summary>
    /// Defines the contract for iteration mode services.
    /// </summary>
    internal interface IIterationModeService
    {
        /// <summary>
        /// Executes the iteration mode asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Int32}"/> representing the asynchronous operation.
        /// The result contains the number of sent requests.
        /// </returns>
        Task<int> ExecuteAsync(CancellationToken cancellationToken);
    }
}
