using LPS.Domain.Common.Interfaces;
using LPS.Domain.Common;
using LPS.Domain;
using LPS.Infrastructure.Caching;
using LPS.Infrastructure.LPSClients.SampleResponseServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using LPS.Infrastructure.LPSClients.URLServices;
using LPS.Infrastructure.LPSClients.SessionManager;
using System.Net;
using LPS.Infrastructure.LPSClients.CachService;
using LPS.Infrastructure.Common.Interfaces;

namespace LPS.Infrastructure.LPSClients.ResponseService
{
    public class ResponseProcessingService(
        ICacheService<string> memoryCacheService,
        ILogger logger,
        IRuntimeOperationIdProvider runtimeOperationIdProvider,
        IResponseProcessorFactory responseProcessorFactory,
        IMetricsService metricsService) : IResponseProcessingService
    {
        private readonly ICacheService<string> _memoryCacheService = memoryCacheService;
        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
        private readonly IResponseProcessorFactory _responseProcessorFactory = responseProcessorFactory;
        private readonly ILogger _logger = logger;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider;
       private readonly IMetricsService _metricsService = metricsService;
        readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        public async Task<(HttpResponse.SetupCommand command, double dataReceivedSize, TimeSpan streamTime)> ProcessResponseAsync(
            HttpResponseMessage responseMessage,
            HttpRequest httpRequest,
            bool cacheResponse,
            CancellationToken token)
        {
            Stopwatch streamStopwatch = Stopwatch.StartNew();
            Stopwatch overAllStopWatch = Stopwatch.StartNew();
            string contentType = responseMessage?.Content?.Headers?.ContentType?.MediaType;
            MimeType mimeType = MimeTypeExtensions.FromContentType(contentType);

            try
            {
                string locationToResponse = string.Empty;
                if (responseMessage.Content == null)
                {
                    throw new InvalidOperationException("Response content is null.");
                }
                var statusLine = $"HTTP/{responseMessage.Version} {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}\r\n";
                long transferredSize = Encoding.UTF8.GetByteCount(statusLine);
                if (!(responseMessage.StatusCode == HttpStatusCode.NotModified || responseMessage.StatusCode == HttpStatusCode.NoContent))
                {
                    // Calculate the headers size (both response and content headers)
                    transferredSize += CalculateHeadersSize(responseMessage);
                    string cacheKey = $"{CachePrefixes.Content}{httpRequest.Id}";
                    string content = await _memoryCacheService.GetItemAsync(cacheKey);

                    using Stream contentStream = await responseMessage.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
                    MemoryStream memoryStream = null;
                    byte[] buffer = _bufferPool.Rent(64000);
                    
                    bool isSemaphoreAcquired = false;
                    try
                    {
                        // Initialize memoryStream if caching is needed
                        if (content == null && cacheResponse)
                        {
                            memoryStream = new MemoryStream();
                        }

                        // Get the response processor
                        IResponseProcessor responseProcessor = await _responseProcessorFactory.CreateResponseProcessorAsync(
                            responseMessage, mimeType, httpRequest.SaveResponse, token);

                        await using (responseProcessor.ConfigureAwait(false))
                        {
                            int bytesRead;
                            streamStopwatch.Start();
                            overAllStopWatch.Start();
                            while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), token)) > 0)
                            {
                                transferredSize += bytesRead;
                                await _metricsService.TryUpdateDataReceivedAsync(httpRequest.Id, bytesRead, streamStopwatch.ElapsedMilliseconds, token);

                                // Write to memoryStream for caching
                                if (memoryStream != null)
                                {
                                    await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                                }

                                // Process the chunk with the responseProcessor
                                await responseProcessor.ProcessResponseChunkAsync(buffer, 0, bytesRead, token);

                                streamStopwatch.Restart();
                            }

                            streamStopwatch.Stop();
                            overAllStopWatch.Stop();
                            // Get the response file path if available
                            locationToResponse = responseProcessor.ResponseFilePath;
                        }

                        // Cache the content once fully read
                        if (memoryStream != null)
                        {
                            content = Encoding.UTF8.GetString(memoryStream.ToArray());
                            await _memoryCacheService.SetItemAsync(cacheKey, content);
                        }
                        await _semaphoreSlim.WaitAsync(token);
                        isSemaphoreAcquired = true;
                    }
                    finally
                    {
                        if (isSemaphoreAcquired)
                            _semaphoreSlim.Release();
                        _bufferPool.Return(buffer);
                        if (memoryStream != null)
                        {
                            await memoryStream.DisposeAsync();
                        }
                    }

                }

                return (new HttpResponse.SetupCommand
                {
                    StatusCode = responseMessage.StatusCode,
                    StatusMessage = responseMessage.ReasonPhrase,
                    LocationToResponse = locationToResponse,
                    IsSuccessStatusCode = responseMessage.IsSuccessStatusCode,
                    ResponseContentHeaders = responseMessage.Content?.Headers?.ToDictionary(header => header.Key, header => string.Join(", ", header.Value)),
                    ResponseHeaders = responseMessage.Headers?.ToDictionary(header => header.Key, header => string.Join(", ", header.Value)),
                    ContentType = mimeType,
                    HttpRequestId = httpRequest.Id,
                }, transferredSize, overAllStopWatch.Elapsed);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Error in ProcessResponseAsync: {ex.Message}", LPSLoggingLevel.Error, token);
                throw;
            }
        }

        private static long CalculateHeadersSize(HttpResponseMessage response)
        {
            long size = 0;

            // Calculate size of response headers
            foreach (var header in response.Headers)
            {
                // Include CRLF at the end of each header line
                size += Encoding.UTF8.GetByteCount($"{header.Key}: {string.Join(", ", header.Value)}\r\n");
            }

            // Calculate size of content headers
            if (response.Content?.Headers != null)
            {
                foreach (var contentHeader in response.Content.Headers)
                {
                    // Include CRLF at the end of each content header line
                    size += Encoding.UTF8.GetByteCount($"{contentHeader.Key}: {string.Join(", ", contentHeader.Value)}\r\n");
                }
            }

            // Include an additional CRLF after the headers section to separate headers from the body
            size += Encoding.UTF8.GetByteCount("\r\n");

            return size;
        }

    }

}
