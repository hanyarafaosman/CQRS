namespace Waseet.CQRS.Authorization;

/// <summary>
/// Exception thrown when a request fails authorization checks.
/// </summary>
public class AuthorizationException : Exception
{
    /// <summary>
    /// Gets the name of the request that failed authorization.
    /// </summary>
    public string RequestName { get; }
    
    /// <summary>
    /// Gets the policy name that was required.
    /// </summary>
    public string? PolicyName { get; }
    
    /// <summary>
    /// Gets the roles that were required.
    /// </summary>
    public string[]? RequiredRoles { get; }
    
    /// <summary>
    /// Gets a value indicating whether the user was authenticated.
    /// </summary>
    public bool WasAuthenticated { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationException"/> class.
    /// </summary>
    /// <param name="requestName">The name of the request that failed authorization.</param>
    /// <param name="policyName">The policy name that was required.</param>
    /// <param name="requiredRoles">The roles that were required.</param>
    /// <param name="wasAuthenticated">Whether the user was authenticated.</param>
    public AuthorizationException(
        string requestName, 
        string? policyName = null, 
        string[]? requiredRoles = null,
        bool wasAuthenticated = false)
        : base(BuildMessage(requestName, policyName, requiredRoles, wasAuthenticated))
    {
        RequestName = requestName;
        PolicyName = policyName;
        RequiredRoles = requiredRoles;
        WasAuthenticated = wasAuthenticated;
    }
    
    private static string BuildMessage(string requestName, string? policyName, string[]? requiredRoles, bool wasAuthenticated)
    {
        if (!wasAuthenticated)
        {
            return $"User is not authenticated. Request '{requestName}' requires authentication.";
        }
        
        if (!string.IsNullOrEmpty(policyName))
        {
            return $"User does not have permission to execute '{requestName}'. Required policy: '{policyName}'.";
        }
        
        if (requiredRoles != null && requiredRoles.Length > 0)
        {
            return $"User does not have permission to execute '{requestName}'. Required roles: {string.Join(", ", requiredRoles)}.";
        }
        
        return $"User does not have permission to execute '{requestName}'.";
    }
}
