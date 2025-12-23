using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace Waseet.CQRS.Caching;

/// <summary>
/// Default in-memory cache provider using MemoryCache.
/// </summary>
public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = new();
    private readonly object _lock = new();

    public MemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = _cache.TryGetValue<T>(key, out var cached) ? cached : default;
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration
        };
        
        _cache.Set(key, value, options);
        TrackKey(key);
        
        return Task.CompletedTask;
    }

    public Task SetWithSlidingAsync<T>(string key, T value, TimeSpan slidingExpiration, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration
        };
        
        _cache.Set(key, value, options);
        TrackKey(key);
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        RemoveTrackedKey(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var regex = new Regex(pattern.Replace("*", ".*"), RegexOptions.IgnoreCase);
        
        lock (_lock)
        {
            var keysToRemove = _keys.Where(k => regex.IsMatch(k)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.Remove(key);
            }
        }
        
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            foreach (var key in _keys.ToList())
            {
                _cache.Remove(key);
            }
            _keys.Clear();
        }
        
        return Task.CompletedTask;
    }

    private void TrackKey(string key)
    {
        lock (_lock)
        {
            _keys.Add(key);
        }
    }

    private void RemoveTrackedKey(string key)
    {
        lock (_lock)
        {
            _keys.Remove(key);
        }
    }
}
