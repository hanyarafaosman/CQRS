namespace Waseet.CQRS.Idempotency;

/// <summary>
/// Marks a command as idempotent, ensuring it executes only once for a given idempotency key.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class IdempotentAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the duration in seconds to store the idempotency key.
    /// Default is 86400 (24 hours).
    /// </summary>
    public int Duration { get; set; } = 86400;
    
    /// <summary>
    /// Gets or sets the property name that contains the idempotency key.
    /// If not specified, looks for property named "IdempotencyKey".
    /// </summary>
    public string? KeyProperty { get; set; }
}
