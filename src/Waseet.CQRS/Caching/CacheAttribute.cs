namespace Waseet.CQRS.Caching;

/// <summary>
/// Marks a request for response caching.
/// Only applies to queries (requests with responses).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class CacheAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the cache key. If not specified, uses request type name and parameters.
    /// Use {PropertyName} to include request properties in the key.
    /// Example: "user-{UserId}" for GetUserQuery with UserId property.
    /// </summary>
    public string? Key { get; set; }
    
    /// <summary>
    /// Gets or sets the cache duration in seconds.
    /// </summary>
    public int Duration { get; set; } = 300; // 5 minutes default
    
    /// <summary>
    /// Gets or sets the sliding expiration in seconds.
    /// If accessed, the cache entry is renewed for this duration.
    /// </summary>
    public int? SlidingExpiration { get; set; }
    
    /// <summary>
    /// Gets or sets whether to cache null responses.
    /// </summary>
    public bool CacheNullValues { get; set; } = false;
}
