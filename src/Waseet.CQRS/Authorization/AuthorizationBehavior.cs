using Microsoft.Extensions.DependencyInjection;

namespace Waseet.CQRS.Authorization;

/// <summary>
/// Pipeline behavior that performs authorization checks before executing the request handler.
/// Checks for [Authorize] attributes on the request and validates against policies and roles.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var authorizeAttributes = requestType.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .ToArray();

        // If no [Authorize] attribute, proceed without checks
        if (authorizeAttributes.Length == 0)
        {
            return await next();
        }

        // Try to get authorization context
        var authContext = _serviceProvider.GetService<IAuthorizationContext>();
        
        // If no authorization context is registered, throw exception
        if (authContext == null)
        {
            throw new InvalidOperationException(
                $"Request '{requestType.Name}' has [Authorize] attribute but no IAuthorizationContext is registered. " +
                "Register an implementation of IAuthorizationContext in your DI container.");
        }

        // Check if user is authenticated (if any authorize attribute exists)
        if (!authContext.IsAuthenticated)
        {
            throw new AuthorizationException(
                requestName: requestType.Name,
                wasAuthenticated: false);
        }

        // Check each authorization requirement
        foreach (var authorizeAttr in authorizeAttributes)
        {
            // Check policy
            if (!string.IsNullOrEmpty(authorizeAttr.Policy))
            {
                var hasPolicy = await authContext.HasPolicyAsync(authorizeAttr.Policy);
                if (!hasPolicy)
                {
                    throw new AuthorizationException(
                        requestName: requestType.Name,
                        policyName: authorizeAttr.Policy,
                        wasAuthenticated: true);
                }
            }
            // Check if policy should default to request name
            else if (string.IsNullOrEmpty(authorizeAttr.Roles))
            {
                // Use request type name as policy name by default
                var defaultPolicy = requestType.Name;
                var hasPolicy = await authContext.HasPolicyAsync(defaultPolicy);
                if (!hasPolicy)
                {
                    throw new AuthorizationException(
                        requestName: requestType.Name,
                        policyName: defaultPolicy,
                        wasAuthenticated: true);
                }
            }

            // Check roles
            if (!string.IsNullOrEmpty(authorizeAttr.Roles))
            {
                var roles = authorizeAttr.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToArray();

                var hasAnyRole = roles.Any(role => authContext.IsInRole(role));
                if (!hasAnyRole)
                {
                    throw new AuthorizationException(
                        requestName: requestType.Name,
                        requiredRoles: roles,
                        wasAuthenticated: true);
                }
            }
        }

        // All authorization checks passed, proceed
        return await next();
    }
}
