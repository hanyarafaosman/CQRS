namespace Waseet.CQRS.Idempotency;

/// <summary>
/// Interface for storing and retrieving idempotency keys and their responses.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Checks if an idempotency key exists and returns the cached response.
    /// </summary>
    Task<IdempotencyResult<TResponse>?> GetAsync<TResponse>(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stores an idempotency key with its response.
    /// </summary>
    Task SetAsync<TResponse>(string key, TResponse response, TimeSpan duration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes an idempotency key from the store.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an idempotency check.
/// </summary>
public class IdempotencyResult<TResponse>
{
    /// <summary>
    /// The cached response.
    /// </summary>
    public TResponse Response { get; set; } = default!;
    
    /// <summary>
    /// When this entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
