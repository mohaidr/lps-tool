// ICacheService.cs
using System;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Caching
{
    public interface ICacheService<TItem>
    {
        Task<TItem> GetItemAsync(string key);
        Task SetItemAsync(string key, TItem item, TimeSpan? duration = null);
        bool TryGetItem(string key, out TItem item);
        Task RemoveItemAsync(string key);
    }
}
