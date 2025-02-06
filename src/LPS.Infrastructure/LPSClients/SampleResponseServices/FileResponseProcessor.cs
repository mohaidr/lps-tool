// FileResponseProcessor.cs
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Caching;
using LPS.Infrastructure.LPSClients.URLServices;
using System.Collections.Concurrent;
using LPS.Infrastructure.LPSClients.CachService;
using System.Net.Http;

namespace LPS.Infrastructure.LPSClients.SampleResponseServices
{
    public class FileResponseProcessor(
        HttpResponseMessage responseMessage,
        ICacheService<string> memoryCache,
        ILogger logger,
        IRuntimeOperationIdProvider runtimeOperationIdProvider,
        IUrlSanitizationService urlSanitizationService) : IResponseProcessor
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreDictionary = new();

        private FileStream _fileStream;
        private readonly ICacheService<string> _memoryCache = memoryCache;
        private readonly ILogger _logger = logger;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider;
        private readonly HttpResponseMessage _message = responseMessage;
        private bool _disposed = false;
        private bool _isInitialized = false;
        readonly IUrlSanitizationService _urlSanitizationService = urlSanitizationService;
        public string ResponseFilePath { get; private set; }
        readonly string _url = responseMessage?.RequestMessage?.RequestUri?.ToString();
        readonly string _cacheKey = $"{CachePrefixes.SampleResponse}{responseMessage?.RequestMessage?.RequestUri?.ToString()}";
        /// <summary>
        /// Initializes the FileStream and updates the cache with no expiration.
        /// This method manages the semaphore internally.
        /// </summary>
        public async Task InitializeAsync(string fileExtension, CancellationToken token)
        {
            var semaphore = _semaphoreDictionary.GetOrAdd(_cacheKey, new SemaphoreSlim(1, 1));
            bool lockAcquired = false;
            try
            {
                await semaphore.WaitAsync(token);
                lockAcquired = true;

                if (_memoryCache.TryGetItem(_cacheKey, out string responseFilePath))
                {
                    _isInitialized = false;
                    ResponseFilePath = responseFilePath;
                    return;
                }
                else
                {
                    // Proceed to initialize processing
                    _isInitialized = true;

                    // Sanitize the URL and prepare the file path
                    string sanitizedUrl = _urlSanitizationService.Sanitize(_url);
                    string directoryName = $"{sanitizedUrl}.{_runtimeOperationIdProvider.OperationId}.Resources";
                    Directory.CreateDirectory(directoryName);
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    string filePath = Path.Combine(directoryName, $"{sanitizedUrl}_{timestamp}{fileExtension}");

                    _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
                    ResponseFilePath = filePath;

                    // Set cache with no expiration (using TimeSpan.MaxValue)
                    await _memoryCache.SetItemAsync(_cacheKey, filePath, TimeSpan.MaxValue);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(
                    _runtimeOperationIdProvider.OperationId,
                    $"Failed to initialize FileResponseProcessor for URL {_url}: {ex.Message}",
                    LPSLoggingLevel.Error,
                    token);

            }
            finally
            {
                if (lockAcquired)
                {
                    semaphore.Release();
                }
            }
        }

        public async Task ProcessResponseChunkAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(FileResponseProcessor));
            if (!_isInitialized)
            {
                // No-op processor; do nothing
                return;
            }
            try
            {
                await _fileStream.WriteAsync(buffer.AsMemory(offset, count), token);
            }
            catch (Exception ex)
            {
                // On failure, remove cache entry and log the error
                await _memoryCache.RemoveItemAsync(_cacheKey);
                await _logger.LogAsync(
                    _runtimeOperationIdProvider.OperationId,
                    $"Failed to write response chunk for URL {_url}: {ex.Message}",
                    LPSLoggingLevel.Error,
                    CancellationToken.None);
            }
        }
        
        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_isInitialized)
                {
                    try
                    {
                        await _fileStream.FlushAsync();
                        await _fileStream.DisposeAsync();

                        // Update cache entry with default cache duration
                        await _memoryCache.SetItemAsync(_cacheKey, _fileStream.Name);
                        await _logger.LogAsync(
                            _runtimeOperationIdProvider.OperationId,
                            $"Sample response saved for URL: {_url}",
                            LPSLoggingLevel.Verbose,
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogAsync(
                            _runtimeOperationIdProvider.OperationId,
                            $"Error during disposal of FileResponseProcessor for URL {_url}: {ex.Message}",
                            LPSLoggingLevel.Error,
                            CancellationToken.None);
                    }
                    finally
                    {
                    }
                }
            }
        }
    }
}
