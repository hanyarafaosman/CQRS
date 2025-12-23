namespace Waseet.CQRS.Monitoring;

/// <summary>
/// Marks a request for performance monitoring.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MonitorAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the threshold in milliseconds for slow request warnings.
    /// Default is 1000ms (1 second).
    /// </summary>
    public int SlowThresholdMs { get; set; } = 1000;
    
    /// <summary>
    /// Gets or sets whether to include request details in monitoring data.
    /// </summary>
    public bool IncludeRequestData { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to include response details in monitoring data.
    /// </summary>
    public bool IncludeResponseData { get; set; } = false;
}
