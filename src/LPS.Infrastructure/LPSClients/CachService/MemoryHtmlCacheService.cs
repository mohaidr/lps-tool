// MemoryHtmlCacheService.cs
using LPS.Infrastructure.LPSClients.CachService;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Caching
{
    public class MemoryHtmlCacheService : MemoryCacheService<string>, IHtmlCacheService
    {
        public MemoryHtmlCacheService(IMemoryCache memoryCache) : base(memoryCache)
        {
        }
    }
}
