// NoOpResponseProcessor.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.SampleResponseServices
{


    public class NoOpResponseProcessor : IResponseProcessor
    {
        public string ResponseFilePath => null;

        public Task ProcessResponseChunkAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            // Nothing to dispose
            return ValueTask.CompletedTask;
        }
    }

}
