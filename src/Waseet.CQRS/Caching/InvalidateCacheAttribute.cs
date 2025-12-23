namespace Waseet.CQRS.Caching;

/// <summary>
/// Marks a command that should invalidate specific cache entries.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class InvalidateCacheAttribute : Attribute
{
    /// <summary>
    /// Gets the cache keys to invalidate.
    /// Can use {PropertyName} to include command properties.
    /// Example: "user-{UserId}" will invalidate cache for specific user.
    /// </summary>
    public string[] Keys { get; }
    
    /// <summary>
    /// Gets or sets whether to use pattern matching for invalidation.
    /// If true, Keys are treated as patterns (e.g., "user-*" invalidates all user caches).
    /// </summary>
    public bool UsePattern { get; set; }
    
    /// <summary>
    /// Initializes a new instance with cache keys to invalidate.
    /// </summary>
    public InvalidateCacheAttribute(params string[] keys)
    {
        Keys = keys ?? Array.Empty<string>();
    }
}
