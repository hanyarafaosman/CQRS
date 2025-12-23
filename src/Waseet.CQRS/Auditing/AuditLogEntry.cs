namespace Waseet.CQRS.Auditing;

/// <summary>
/// Represents an audit log entry.
/// </summary>
public class AuditLogEntry
{
    /// <summary>
    /// Unique identifier for this audit entry.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Timestamp when the action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Type of request that was executed.
    /// </summary>
    public string RequestType { get; set; } = string.Empty;
    
    /// <summary>
    /// User who executed the request (if available).
    /// </summary>
    public string? User { get; set; }
    
    /// <summary>
    /// Request data (serialized).
    /// </summary>
    public object? RequestData { get; set; }
    
    /// <summary>
    /// Response data (serialized).
    /// </summary>
    public object? ResponseData { get; set; }
    
    /// <summary>
    /// Execution duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }
    
    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if the request failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Category/type of audit entry.
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Tags for filtering and searching.
    /// </summary>
    public string[]? Tags { get; set; }
    
    /// <summary>
    /// IP address of the client (if available).
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Additional custom data.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
