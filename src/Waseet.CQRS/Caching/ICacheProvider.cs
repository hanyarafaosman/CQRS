namespace Waseet.CQRS.Caching;

/// <summary>
/// Provides caching functionality for request responses.
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets a cached value with absolute expiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets a cached value with sliding expiration.
    /// </summary>
    Task SetWithSlidingAsync<T>(string key, T value, TimeSpan slidingExpiration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a cached value by key.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes all cached values matching the pattern.
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clears all cached values.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
