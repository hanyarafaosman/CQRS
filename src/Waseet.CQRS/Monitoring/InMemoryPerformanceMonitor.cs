using System.Collections.Concurrent;

namespace Waseet.CQRS.Monitoring;

/// <summary>
/// Default in-memory performance monitor.
/// </summary>
public class InMemoryPerformanceMonitor : IPerformanceMonitor
{
    private readonly ConcurrentBag<PerformanceMetrics> _metrics = new();
    private readonly int _maxMetrics;

    public InMemoryPerformanceMonitor(int maxMetrics = 10000)
    {
        _maxMetrics = maxMetrics;
    }

    public Task RecordAsync(PerformanceMetrics metrics, CancellationToken cancellationToken = default)
    {
        _metrics.Add(metrics);
        
        // Trim old metrics if exceeding max
        while (_metrics.Count > _maxMetrics)
        {
            _metrics.TryTake(out _);
        }
        
        return Task.CompletedTask;
    }

    public Task<PerformanceStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var allMetrics = _metrics.ToArray();
        
        if (allMetrics.Length == 0)
        {
            return Task.FromResult(new PerformanceStatistics());
        }

        var stats = new PerformanceStatistics
        {
            TotalRequests = allMetrics.Length,
            SuccessfulRequests = allMetrics.Count(m => m.Success),
            FailedRequests = allMetrics.Count(m => !m.Success),
            AverageDurationMs = allMetrics.Average(m => m.DurationMs),
            MinDurationMs = allMetrics.Min(m => m.DurationMs),
            MaxDurationMs = allMetrics.Max(m => m.DurationMs),
            SlowRequestCount = allMetrics.Count(m => m.IsSlow)
        };

        return Task.FromResult(stats);
    }

    public Task<PerformanceStatistics?> GetStatisticsAsync(string requestName, CancellationToken cancellationToken = default)
    {
        var requestMetrics = _metrics.Where(m => m.RequestName == requestName).ToArray();
        
        if (requestMetrics.Length == 0)
        {
            return Task.FromResult<PerformanceStatistics?>(null);
        }

        var stats = new PerformanceStatistics
        {
            RequestName = requestName,
            TotalRequests = requestMetrics.Length,
            SuccessfulRequests = requestMetrics.Count(m => m.Success),
            FailedRequests = requestMetrics.Count(m => !m.Success),
            AverageDurationMs = requestMetrics.Average(m => m.DurationMs),
            MinDurationMs = requestMetrics.Min(m => m.DurationMs),
            MaxDurationMs = requestMetrics.Max(m => m.DurationMs),
            SlowRequestCount = requestMetrics.Count(m => m.IsSlow)
        };

        return Task.FromResult<PerformanceStatistics?>(stats);
    }
}
