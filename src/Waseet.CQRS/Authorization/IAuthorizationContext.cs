using System.Security.Claims;

namespace Waseet.CQRS.Authorization;

/// <summary>
/// Provides access to the current user's identity and claims for authorization checks.
/// </summary>
public interface IAuthorizationContext
{
    /// <summary>
    /// Gets the current user's claims principal.
    /// </summary>
    ClaimsPrincipal? User { get; }
    
    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Checks if the current user has the specified policy.
    /// </summary>
    /// <param name="policyName">The name of the policy to check.</param>
    /// <returns>True if the user has the policy; otherwise, false.</returns>
    Task<bool> HasPolicyAsync(string policyName);
    
    /// <summary>
    /// Checks if the current user has the specified role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role; otherwise, false.</returns>
    bool IsInRole(string role);
}
