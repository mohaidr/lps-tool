// MemoryCacheService.cs
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Caching
{
    public class MemoryCacheService<T>(IMemoryCache memoryCache,
        TimeSpan? defaultCacheDuration = null) : ICacheService<T>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly TimeSpan _defaultCacheDuration = defaultCacheDuration ?? TimeSpan.FromSeconds(30);

        public Task<T> GetItemAsync(string key)
        {
            _memoryCache.TryGetValue(key, out T item);
            return Task.FromResult(item);
        }

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task SetItemAsync(string key, T item, TimeSpan? duration = null)
        {
            bool semaphoreAcquired = false;
            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1); // Set size for memory management

                if (duration == TimeSpan.MaxValue)
                {
                    // Set priority to prevent eviction and avoid expiration
                    cacheEntryOptions.SetPriority(CacheItemPriority.NeverRemove);
                }
                else
                {
                    // Use provided duration or default duration
                    var cacheDuration = duration ?? _defaultCacheDuration;
                    cacheEntryOptions.SetAbsoluteExpiration(DateTimeOffset.UtcNow.Add(cacheDuration));
                }

                // Ensure thread-safety using a semaphore
                await _semaphore.WaitAsync();
                semaphoreAcquired = true;
                _memoryCache.Set(key, item, cacheEntryOptions);
            }
            finally
            {
                if (semaphoreAcquired)
                {
                    _semaphore.Release();
                }
            }
        }


        public bool TryGetItem(string key, out T item)
        {
            return _memoryCache.TryGetValue(key, out item);
        }

        public async Task RemoveItemAsync(string key)
        {
            await Task.Run(() => _memoryCache.Remove(key));
        }
    }
}
