using System.Security.Claims;

namespace Waseet.CQRS.Authorization;

/// <summary>
/// Default implementation of IAuthorizationContext for testing and simple scenarios.
/// In real applications, replace this with your own implementation that integrates
/// with ASP.NET Core authentication or your custom authentication system.
/// </summary>
public class DefaultAuthorizationContext : IAuthorizationContext
{
    private readonly ClaimsPrincipal? _user;
    private readonly HashSet<string> _policies;

    /// <summary>
    /// Gets the current user's claims principal.
    /// </summary>
    public ClaimsPrincipal? User => _user;

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAuthorizationContext"/> class.
    /// </summary>
    public DefaultAuthorizationContext()
    {
        _policies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance with a user and allowed policies.
    /// </summary>
    /// <param name="user">The claims principal representing the user.</param>
    /// <param name="allowedPolicies">The policies the user has access to.</param>
    public DefaultAuthorizationContext(ClaimsPrincipal? user, params string[] allowedPolicies)
    {
        _user = user;
        _policies = new HashSet<string>(allowedPolicies, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the current user has the specified policy.
    /// </summary>
    public Task<bool> HasPolicyAsync(string policyName)
    {
        return Task.FromResult(_policies.Contains(policyName));
    }

    /// <summary>
    /// Checks if the current user is in the specified role.
    /// </summary>
    public bool IsInRole(string role)
    {
        return _user?.IsInRole(role) ?? false;
    }

    /// <summary>
    /// Grants the user access to the specified policy.
    /// </summary>
    public void GrantPolicy(string policyName)
    {
        _policies.Add(policyName);
    }

    /// <summary>
    /// Revokes the user's access to the specified policy.
    /// </summary>
    public void RevokePolicy(string policyName)
    {
        _policies.Remove(policyName);
    }
}
