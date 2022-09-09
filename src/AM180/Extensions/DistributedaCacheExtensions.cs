using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AM180.Extensions
{
    public static class DistributedaCacheExtensions
    {
        public static async Task SetRecordAsync<T>(this IDistributedCache cache,
            string recordId,
            T data,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60),
                SlidingExpiration = unusedExpireTime
            };
            var json = JsonSerializer.Serialize(data);
            await cache.SetStringAsync(recordId, json, options);
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache,
            string recordId)
        {
            var json = await cache.GetStringAsync(recordId);
            if (json == null)
                return default(T);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
