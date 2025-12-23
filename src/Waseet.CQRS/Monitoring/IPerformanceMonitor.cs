namespace Waseet.CQRS.Monitoring;

/// <summary>
/// Interface for performance monitoring.
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// Records performance metrics for a request.
    /// </summary>
    Task RecordAsync(PerformanceMetrics metrics, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets aggregated metrics for all requests.
    /// </summary>
    Task<PerformanceStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets aggregated metrics for a specific request type.
    /// </summary>
    Task<PerformanceStatistics?> GetStatisticsAsync(string requestName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregated performance statistics.
/// </summary>
public class PerformanceStatistics
{
    public string? RequestName { get; set; }
    public long TotalRequests { get; set; }
    public long SuccessfulRequests { get; set; }
    public long FailedRequests { get; set; }
    public double AverageDurationMs { get; set; }
    public long MinDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public long SlowRequestCount { get; set; }
    public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;
}
