using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using System.Threading;
using YamlDotNet.Core.Tokens;
using System.Diagnostics;

namespace LPS.Infrastructure.LPSClients.MessageServices
{
    public class ProgressContent : HttpContent
    {
        private readonly HttpContent _originalContent;
        private readonly IProgress<long> _progress;
        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
        CancellationToken _token;
        Stopwatch _stopwatch;
        public ProgressContent(HttpContent content, IProgress<long> progress, Stopwatch stopwatch, CancellationToken token)
        {
            _token = token;
            _stopwatch = stopwatch;
            _originalContent = content ?? throw new ArgumentNullException(nameof(content));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            foreach (var header in _originalContent.Headers)
            {
                Headers.Add(header.Key, header.Value);
            }
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = _bufferPool.Rent(64000); // Rent 64 KB buffer
            try
            {
                _stopwatch.Start();
                using var contentStream = await _originalContent.ReadAsStreamAsync(_token);
                long totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), _token)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead, _token);
                    totalBytesRead += bytesRead;
                    _progress.Report(bytesRead);
                }
                _stopwatch.Stop();
            }
            finally
            {
                _bufferPool.Return(buffer); // Return the buffer to the pool
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            if (_originalContent.Headers.ContentLength.HasValue)
            {
                length = _originalContent.Headers.ContentLength.Value;
                return true;
            }

            length = -1;
            return false;
        }
    }
}
