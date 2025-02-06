using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.SampleResponseServices
{

    public interface IResponseProcessor : IAsyncDisposable
    {
        /// <summary>
        /// Processes a chunk of the response data.
        /// </summary>
        /// <param name="buffer">The buffer containing response data.</param>
        /// <param name="offset">The zero-based byte offset in the buffer.</param>
        /// <param name="count">The number of bytes to process.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ProcessResponseChunkAsync(byte[] buffer, int offset, int count, CancellationToken token);

        /// <summary>
        /// Gets the file path where the response is saved, if applicable.
        /// </summary>
        string ResponseFilePath { get; }
    }

}
