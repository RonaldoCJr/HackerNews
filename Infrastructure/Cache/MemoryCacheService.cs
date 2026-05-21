using HackerNews.Application.Interfaces;
using HackerNews.Infrastructure.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration;

    public MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> options)
    {
        _cache = cache;
        _defaultExpiration = TimeSpan.FromMinutes(options.Value.DefaultExpirationMinutes);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out T value))
            return value;

        value = await factory();

        if (value != null)
            _cache.Set(key, value, expiration ?? _defaultExpiration);

        return value;
    }
}