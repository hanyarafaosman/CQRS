using System.Collections.Concurrent;

namespace Waseet.CQRS.Idempotency;

/// <summary>
/// In-memory idempotency store using ConcurrentDictionary.
/// </summary>
public class MemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, (object Response, DateTime ExpiresAt, DateTime CreatedAt)> _store = new();
    private readonly Timer _cleanupTimer;

    public MemoryIdempotencyStore()
    {
        // Cleanup expired entries every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<IdempotencyResult<TResponse>?> GetAsync<TResponse>(string key, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult<IdempotencyResult<TResponse>?>(new IdempotencyResult<TResponse>
                {
                    Response = (TResponse)entry.Response,
                    CreatedAt = entry.CreatedAt
                });
            }
            
            // Expired, remove it
            _store.TryRemove(key, out _);
        }
        
        return Task.FromResult<IdempotencyResult<TResponse>?>(null);
    }

    public Task SetAsync<TResponse>(string key, TResponse response, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTime.UtcNow.Add(duration);
        var createdAt = DateTime.UtcNow;
        
        _store[key] = (response!, expiresAt, createdAt);
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private void CleanupExpiredEntries(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _store.Where(kvp => kvp.Value.ExpiresAt <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _store.TryRemove(key, out _);
        }
    }
}
