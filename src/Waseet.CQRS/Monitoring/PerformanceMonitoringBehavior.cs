using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Waseet.CQRS.Monitoring;

/// <summary>
/// Pipeline behavior that monitors request performance.
/// </summary>
public class PerformanceMonitoringBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public PerformanceMonitoringBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var monitorAttr = requestType.GetCustomAttributes(typeof(MonitorAttribute), true)
            .Cast<MonitorAttribute>()
            .FirstOrDefault();

        // If no monitoring attribute and no global monitor, skip
        var monitor = _serviceProvider.GetService<IPerformanceMonitor>();
        if (monitorAttr == null && monitor == null)
        {
            return await next();
        }

        // Start timing
        var stopwatch = Stopwatch.StartNew();
        TResponse? response = default;
        Exception? exception = null;
        bool success = false;

        try
        {
            response = await next();
            success = true;
            return response;
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            if (monitor != null)
            {
                var metrics = new PerformanceMetrics
                {
                    RequestName = requestType.Name,
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    Success = success,
                    ErrorMessage = exception?.Message,
                    Timestamp = DateTime.UtcNow,
                    IsSlow = stopwatch.ElapsedMilliseconds > (monitorAttr?.SlowThresholdMs ?? 1000),
                    RequestData = monitorAttr?.IncludeRequestData == true ? request : null,
                    ResponseData = monitorAttr?.IncludeResponseData == true ? response : null
                };

                await monitor.RecordAsync(metrics, cancellationToken);

                // Log slow requests
                if (metrics.IsSlow)
                {
                    Console.WriteLine($"⚠️ Slow request: {requestType.Name} took {stopwatch.ElapsedMilliseconds}ms");
                }
            }
        }
    }
}
