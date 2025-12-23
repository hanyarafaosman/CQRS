namespace Waseet.CQRS.Authorization;

/// <summary>
/// Specifies that the request requires authorization.
/// The policy name will be matched against the request type name if not specified.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the policy name required for authorization.
    /// If not specified, the request type name will be used as the policy name.
    /// </summary>
    public string? Policy { get; set; }
    
    /// <summary>
    /// Gets or sets the roles that are authorized to execute this request.
    /// Multiple roles can be specified separated by commas.
    /// </summary>
    public string? Roles { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class.
    /// </summary>
    public AuthorizeAttribute()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class with the specified policy.
    /// </summary>
    /// <param name="policy">The policy name required for authorization.</param>
    public AuthorizeAttribute(string policy)
    {
        Policy = policy;
    }
}
