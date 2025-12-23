namespace Waseet.CQRS.Auditing;

/// <summary>
/// Interface for audit logging.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an audit entry.
    /// </summary>
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Queries audit logs (optional - for reading back logs).
    /// </summary>
    Task<IEnumerable<AuditLogEntry>> QueryAsync(AuditQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query parameters for audit logs.
/// </summary>
public class AuditQuery
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? User { get; set; }
    public string? RequestType { get; set; }
    public string? Category { get; set; }
    public bool? Success { get; set; }
    public int PageSize { get; set; } = 100;
    public int Page { get; set; } = 1;
}
