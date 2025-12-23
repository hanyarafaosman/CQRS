using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Waseet.CQRS.Auditing;

/// <summary>
/// Pipeline behavior that handles audit logging.
/// </summary>
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public AuditBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var auditAttr = requestType.GetCustomAttributes(typeof(AuditAttribute), true)
            .Cast<AuditAttribute>()
            .FirstOrDefault();

        if (auditAttr == null)
        {
            return await next();
        }

        var auditLogger = _serviceProvider.GetService<IAuditLogger>();
        if (auditLogger == null)
        {
            return await next();
        }

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

            var entry = new AuditLogEntry
            {
                Timestamp = DateTime.UtcNow,
                RequestType = requestType.Name,
                User = GetCurrentUser(),
                RequestData = auditAttr.IncludeRequest ? request : null,
                ResponseData = auditAttr.IncludeResponse ? response : null,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Success = success,
                ErrorMessage = exception?.Message,
                Category = auditAttr.Category,
                Tags = auditAttr.Tags,
                IpAddress = GetCurrentIpAddress()
            };

            await auditLogger.LogAsync(entry, cancellationToken);
        }
    }

    private string? GetCurrentUser()
    {
        // Try to get from authorization context
        var authContext = _serviceProvider.GetService<Authorization.IAuthorizationContext>();
        return authContext?.User?.Identity?.Name;
    }

    private string? GetCurrentIpAddress()
    {
        // IP address detection requires ASP.NET Core dependency
        // To keep core library lightweight, IP is null by default
        // Users can implement custom audit logger to capture IP from their context
        return null;
    }
}
