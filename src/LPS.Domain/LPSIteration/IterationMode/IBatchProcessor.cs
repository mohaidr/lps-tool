using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRun.IterationMode
{
    /// <summary>
    /// Defines the contract for processing batches of asynchronous commands.
    /// </summary>
    /// <typeparam name="TExecuteCommand">
    /// The type of the execute command, which must implement <see cref="IAsyncCommand{TDomainEntity}"/>.
    /// </typeparam>
    /// <typeparam name="TDomainEntity">
    /// The type of the domain entity, which must implement <see cref="IDomainEntity"/>.
    /// </typeparam>
    internal interface IBatchProcessor<TExucuteCommand, TDomainEntity> where TExucuteCommand : IAsyncCommand<TDomainEntity> where TDomainEntity : IDomainEntity
    {
        /// <summary>
        /// Sends a batch of asynchronous requests based on the provided command, batch size, and condition.
        /// </summary>
        /// <param name="command">
        /// The execute command that defines the asynchronous operation to be performed for each request in the batch.
        /// </param>
        /// <param name="batchSize">
        /// The maximum number of requests to include in a single batch.
        /// </param>
        /// <param name="batchCondition">
        /// A function that determines whether the batch should continue processing based on a condition.
        /// Returns <c>true</c> to continue processing the batch; otherwise, <c>false</c> to stop.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Int32}"/> representing the asynchronous operation.
        /// The result contains the number of requests sent in the batch.
        /// </returns>
        public Task<int> SendBatchAsync(TExucuteCommand command, int batchSize, Func<bool> batchCondition, CancellationToken token);

    }
}
