namespace Waseet.CQRS.Auditing;

/// <summary>
/// Marks a request for auditing/logging.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AuditAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to include request data in the audit log.
    /// </summary>
    public bool IncludeRequest { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to include response data in the audit log.
    /// </summary>
    public bool IncludeResponse { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the category/type for this audit entry.
    /// Useful for filtering in Elasticsearch.
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Gets or sets additional tags for this audit entry.
    /// </summary>
    public string[]? Tags { get; set; }
}
