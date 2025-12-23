namespace Waseet.CQRS.Monitoring;

/// <summary>
/// Performance metrics for a request.
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Request type name.
    /// </summary>
    public string RequestName { get; set; } = string.Empty;
    
    /// <summary>
    /// Execution duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }
    
    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Exception message if failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Timestamp when the request was executed.
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Whether this request was considered slow.
    /// </summary>
    public bool IsSlow { get; set; }
    
    /// <summary>
    /// Request data (if included).
    /// </summary>
    public object? RequestData { get; set; }
    
    /// <summary>
    /// Response data (if included).
    /// </summary>
    public object? ResponseData { get; set; }
}
