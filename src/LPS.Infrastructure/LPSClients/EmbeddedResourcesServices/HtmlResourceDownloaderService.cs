// HtmlResourceDownloaderService.cs
using HtmlAgilityPack;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Caching;
using LPS.Infrastructure.Logger;
using LPS.Infrastructure.LPSClients.URLServices;
using LPS.Infrastructure.Monitoring.Metrics;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using LPS.Infrastructure.LPSClients.CachService;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Diagnostics;
using LPS.Infrastructure.Common.Interfaces;

namespace LPS.Infrastructure.LPSClients.EmbeddedResourcesServices
{
    public class HtmlResourceDownloaderService : IHtmlResourceDownloaderService
    {
        private readonly ILogger _logger;
        private readonly IRuntimeOperationIdProvider _operationIdProvider;
        private readonly HttpClient _httpClient;
        private readonly ICacheService<string> _memoryCacheService;
        private const int _bufferSize = 8 * 1024;
        private readonly IMetricsService _metricsService;
        public HtmlResourceDownloaderService(
            ILogger logger,
            IRuntimeOperationIdProvider operationIdProvider,
            HttpClient httpClient,
            IMetricsService metricsService,
            ICacheService<string> memoryCacheService)
        {
            _logger = logger;
            _operationIdProvider = operationIdProvider;
            _httpClient = httpClient;
            _memoryCacheService = memoryCacheService;
            _metricsService = metricsService;
        }

        public async Task DownloadResourcesAsync(
            string baseUrl,
            Guid requestId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _logger.LogAsync(_operationIdProvider.OperationId, $"Starting resource download for {requestId}", LPSLoggingLevel.Verbose, cancellationToken);

                // Cache key for the resource URLs
                string resourceUrlsCacheKey = $"{CachePrefixes.ResourceUrls}{requestId}";

                // Try to get the cached resource URLs from IHtmlCacheService
                string cachedResourceUrls = await _memoryCacheService.GetItemAsync(resourceUrlsCacheKey);
                List<string> resourceUrls;

                // If the resource URLs are cached, deserialize them
                if (!(cachedResourceUrls == null))
                {
                    resourceUrls = cachedResourceUrls.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    await _logger.LogAsync(_operationIdProvider.OperationId, "Resource URLs retrieved from cache.", LPSLoggingLevel.Verbose, cancellationToken);
                }
                else
                {
                    // Retrieve HTML content if resource URLs are not cached
                    string contentCacheKey = $"{CachePrefixes.Content}{requestId}";
                    string htmlContent = await _memoryCacheService.GetItemAsync(contentCacheKey);
                    if (string.IsNullOrEmpty(htmlContent))
                    {
                        await _logger.LogAsync(_operationIdProvider.OperationId, "No cached item found of type html.", LPSLoggingLevel.Warning, cancellationToken);
                        return;
                    }
                    await _logger.LogAsync(_operationIdProvider.OperationId, $"No URLs cached for {baseUrl}.", LPSLoggingLevel.Warning, cancellationToken);

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    var resourceSelectors = new[]
                    {
                        "//img[@src]",
                        "//link[@rel='stylesheet' and @href]",
                        "//script[@src]"
                    };

                    // Extract resource URLs
                    resourceUrls = resourceSelectors
                        .SelectMany(xpath => doc.DocumentNode.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>())
                        .Select(node =>
                        {
                            if (node.Name.Equals("img", StringComparison.OrdinalIgnoreCase) ||
                                node.Name.Equals("script", StringComparison.OrdinalIgnoreCase))
                            {
                                return node.GetAttributeValue("src", null);
                            }
                            else if (node.Name.Equals("link", StringComparison.OrdinalIgnoreCase))
                            {
                                return node.GetAttributeValue("href", null);
                            }
                            return null;
                        })
                        .Where(url => !string.IsNullOrEmpty(url) && !url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                        .Distinct()
                        .ToList();

                    // Cache the extracted resource URLs as a comma-separated string
                    string serializedResourceUrls = string.Join(",", resourceUrls);
                    await _memoryCacheService.SetItemAsync(resourceUrlsCacheKey, serializedResourceUrls);

                    await _logger.LogAsync(_operationIdProvider.OperationId, $"Extracted and cached {resourceUrls.Count} resource URLs.", LPSLoggingLevel.Verbose, cancellationToken);
                }

                await _logger.LogAsync(_operationIdProvider.OperationId, $"Found {resourceUrls.Count} resources to download.", LPSLoggingLevel.Verbose, cancellationToken);

                int maxDegreeOfParallelism = 5000;
                using (SemaphoreSlim semaphore = new SemaphoreSlim(maxDegreeOfParallelism))
                {
                    var downloadTasks = resourceUrls.Select(async resourceUrl =>
                    {
                        bool semaphoreAcquired = false;
                        await semaphore.WaitAsync(cancellationToken);
                        semaphoreAcquired = true;
                        try
                        {
                            await DownloadResourceAsync(baseUrl, requestId, resourceUrl, cancellationToken);
                        }
                        finally
                        {
                            if (semaphoreAcquired)
                            {
                                semaphore.Release();
                            }
                        }
                    });

                    await Task.WhenAll(downloadTasks);
                }

                await _logger.LogAsync(_operationIdProvider.OperationId, $"Completed resource download for {requestId}", LPSLoggingLevel.Verbose, cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_operationIdProvider.OperationId, $"Error in DownloadResourcesAsync: {ex.Message}", LPSLoggingLevel.Error, cancellationToken);
                throw;
            }
        }


        private async Task DownloadResourceAsync(string baseUrl, Guid requestId, string resourceUrl, CancellationToken cancellationToken)
        {
            Stopwatch timeToDownloadWatch = new Stopwatch();
            try
            {
                Uri resourceUri = new Uri(new Uri(baseUrl), resourceUrl);

                using (HttpResponseMessage response = await _httpClient.GetAsync(resourceUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    using (Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                    {
                        byte[] buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
                        try
                        {
                            timeToDownloadWatch.Start();
                            int totalBytes = 0;
                            int bytesRead;
                            while ((bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, _bufferSize), cancellationToken)) > 0)
                            {
                                totalBytes += bytesRead;
                                // Process bytes if needed
                            }
                            timeToDownloadWatch.Stop();
                            await _metricsService.TryUpdateDataReceivedAsync(requestId, totalBytes, timeToDownloadWatch.ElapsedMilliseconds, cancellationToken);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }
                }

                await _logger.LogAsync(_operationIdProvider.OperationId, $"Downloaded resource: {resourceUri}", LPSLoggingLevel.Verbose, cancellationToken);
            }
            catch (OperationCanceledException oce) when (!cancellationToken.IsCancellationRequested)
            {
                await _logger.LogAsync(_operationIdProvider.OperationId, $"Download timed out for resource {resourceUrl}: {oce.Message}", LPSLoggingLevel.Error, cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_operationIdProvider.OperationId, $"Failed to download resource {resourceUrl}: {ex.Message}", LPSLoggingLevel.Error, cancellationToken);
            }
        }
    }
}
