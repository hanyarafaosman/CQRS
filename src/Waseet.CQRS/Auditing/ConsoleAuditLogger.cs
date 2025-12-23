using System.Text.Json;

namespace Waseet.CQRS.Auditing;

/// <summary>
/// Default console-based audit logger.
/// </summary>
public class ConsoleAuditLogger : IAuditLogger
{
    public Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var color = entry.Success ? ConsoleColor.Green : ConsoleColor.Red;
        var originalColor = Console.ForegroundColor;
        
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[AUDIT] {entry.Timestamp:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Type: {entry.RequestType}");
            Console.WriteLine($"  User: {entry.User ?? "Anonymous"}");
            Console.WriteLine($"  Duration: {entry.DurationMs}ms");
            Console.WriteLine($"  Success: {entry.Success}");
            
            if (!entry.Success && !string.IsNullOrEmpty(entry.ErrorMessage))
            {
                Console.WriteLine($"  Error: {entry.ErrorMessage}");
            }
            
            if (entry.RequestData != null)
            {
                Console.WriteLine($"  Request: {JsonSerializer.Serialize(entry.RequestData)}");
            }
            
            if (entry.ResponseData != null)
            {
                Console.WriteLine($"  Response: {JsonSerializer.Serialize(entry.ResponseData)}");
            }
            
            Console.WriteLine();
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
        
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AuditLogEntry>> QueryAsync(AuditQuery query, CancellationToken cancellationToken = default)
    {
        // Console logger doesn't support querying
        return Task.FromResult(Enumerable.Empty<AuditLogEntry>());
    }
}
