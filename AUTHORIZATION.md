# Authorization in Waseet.CQRS

Waseet.CQRS provides built-in authorization support through pipeline behaviors, similar to validation. Authorization checks are performed automatically before executing request handlers.

## Features

- **Policy-Based Authorization**: Check if users have specific policies
- **Role-Based Authorization**: Verify user roles before execution
- **Automatic Policy Matching**: Uses request type name as policy if not specified
- **Pipeline Integration**: Executes before validation and handler execution
- **Comprehensive Error Messages**: Clear feedback when authorization fails

## Quick Start

### ONE-TIME Setup (Register Once, Works for All Requests)

The key point: You register `IAuthorizationContext` **ONCE** in your application startup. It then automatically works for **ALL** your authorized requests.

### For ASP.NET Core (Recommended - Simplest)

Create your authorization context once:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Waseet.CQRS.Authorization;
using System.Security.Claims;

// Create this class ONCE in your project
public class HttpContextAuthorizationContext : IAuthorizationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public HttpContextAuthorizationContext(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public async Task<bool> HasPolicyAsync(string policyName)
    {
        if (User == null) return false;
        var result = await _authorizationService.AuthorizeAsync(User, policyName);
        return result.Succeeded;
    }

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
```

Then register it ONCE in Program.cs:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* your JWT config */);

// 2. Define authorization policies (ONCE)
builder.Services.AddAuthorization(options =>
{
    // Policy names match your command/query names
    options.AddPolicy("DeleteUserCommand", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("GetSensitiveDataQuery", policy =>
        policy.RequireClaim("DataAccess", "Sensitive"));
});

// 3. Register Waseet.CQRS
builder.Services.AddMediator(typeof(Program).Assembly);
builder.Services.AddMediatorValidation(typeof(Program).Assembly);

// 4. Register authorization context ONCE (works for ALL requests automatically!)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationContext, HttpContextAuthorizationContext>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

**That's it!** Now ALL your requests with `[Authorize]` will work automatically. The authorization context gets the current user from HttpContext automatically for every request.

### For Non-ASP.NET Core Applications

If you're not using ASP.NET Core, register your own implementation once:

```csharp
// Register ONCE in your application startup
services.AddSingleton<IAuthorizationContext, YourAuthorizationContext>();
services.AddWaseetCQRS(typeof(Program).Assembly);

// Now it works for ALL requests automatically
```

```csharp
using Waseet.CQRS;
using Waseet.CQRS.Authorization;

// Requires specific policy
[Authorize(Policy = "CanDeleteUsers")]
public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

// Requires specific role
[Authorize(Roles = "Admin")]
public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

// Uses request name as policy (checks for "GetSensitiveDataQuery" policy)
[Authorize]
public record GetSensitiveDataQuery : IRequest<string>;

// Multiple authorization requirements
[Authorize(Policy = "CanViewReports")]
[Authorize(Roles = "Manager,Admin")]
public record GetFinancialReportQuery : IRequest<Report>;
### Mark Your Requests with [Authorize] Attribute

After the one-time setup above, just add `[Authorize]` to your requests:

## How Authorization Context Works

### You Register It ONCE

```csharp
// In Program.cs or Startup.cs - register ONCE
services.AddScoped<IAuthorizationContext, HttpContextAuthorizationContext>();
```

### It Works for ALL Requests Automatically

```csharp
// Request 1
[Authorize(Roles = "Admin")]
public record DeleteUserCommand(Guid Id) : IRequest<Unit>;

// Request 2
[Authorize(Policy = "CanViewReports")]
public record GetReportQuery(int Id) : IRequest<Report>;

// Request 3
[Authorize]
public record GetSensitiveDataQuery : IRequest<string>;

// All three requests use the SAME IAuthorizationContext instance!
// No need to register anything per request.
```

### How It Gets the Current User

For each HTTP request:
1. User sends HTTP request with auth token (JWT, Cookie, etc.)
2. ASP.NET Core authenticates the user
3. When you call `mediator.Send()`:
   - `IAuthorizationContext` is resolved from DI
   - It automatically gets current user from `HttpContext`
   - Checks authorization requirements
   - Proceeds or throws exception
    }
}

// ASP.NET Core - register ONCE, works automatically
services.AddHttpContextAccessor();
services.AddScoped<IAuthorizationContext, HttpContextAuthorizationContext>();

// OR use the convenience extension (includes everything)
services.AddWaseetCQRSWithAspNetCore(typeof(Program).Assembly);

// For testing only - manually specify user and policies
services.AddScoped<IAuthorizationContext>(_ => 
    new DefaultAuthorizationContext(testUser, "Policy1", "Policy2"));
```

## Real-World Example

### Complete ASP.NET Core Setup

```csharp
// Program.cs - Setup happens ONCE
var builder = WebApplication.CreateBuilder(args);

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* config */);

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DeleteUserCommand", p => p.RequireRole("Admin"));
    options.AddPolicy("ViewReportsQuery", p => p.RequireRole("Manager"));
    options.AddPolicy("CanManageUsers", p => p.RequireRole("Admin", "Manager"));
});

// Register Waseet.CQRS with authorization (ONE LINE)
builder.Services.AddWaseetCQRSWithAspNetCore(typeof(Program).Assembly);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Your Commands/Queries

```csharp
// Just add [Authorize] - authorization happens automatically!
[Authorize] // Uses "DeleteUserCommand" policy
public record DeleteUserCommand(Guid Id) : IRequest<Unit>;

[Authorize(Roles = "Admin,Manager")]
public record ViewReportsQuery : IRequest<List<Report>>;

[Authorize(Policy = "CanManageUsers")]
public record CreateUserCommand(string Name) : IRequest<Guid>;
```

### Your Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator; // Authorization context injected automatically
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            // Authorization happens automatically!
            // No need to pass user, check permissions, or do anything special
            await _mediator.Send(new DeleteUserCommand(id));
            return Ok();
        }
        catch (AuthorizationException)
        {
            return Forbid(); // 403
        }
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports()
    {
        try
        {
            // Again, authorization is automatic
            var reports = await _mediator.Send(new ViewReportsQuery());
            return Ok(reports);
        }
        catch (AuthorizationException)
        {
            return Forbid();
        }
    }
}
```

**Key Point:** Notice how the controller doesn't:
- Create authorization context
- Pass user information
- Check permissions manually
- Configure anything per request

It just calls `mediator.Send()` and authorization happens automatically based on the `[Authorize]` attribute on your request!

## For Testing

```csharp
services.AddWaseetCQRS(typeof(Program).Assembly);

// Or use the convenience extension (includes everything)
services.AddMediator(typeof(Program).Assembly);
services.AddMediatorValidation(typeof(Program).Assembly);

// For testing - use DefaultAuthorizationContext with test user
services.AddScoped<IAuthorizationContext>(_ => 
    new DefaultAuthorizationContext(tes
```csharp
// DON'T DO THIS - You don't need to register per request!
services.AddScoped<IAuthorizationContext>(_ => 
    new DefaultAuthorizationContext(user, "DeleteUserCommand"));

services.AddScoped<IAuthorizationContext>(_ => 
    new DefaultAuthorizationContext(user, "GetReportQuery"));

services.AddScoped<IAuthorizationContext>(_ => 
    new DefaultAuthorizationContext(user, "UpdateUserCommand"));
```

### ✅ CORRECT - Register once, define policies:

```csharp
// 1. Register authorization context ONCE
services.AddScoped<IAuthorizationContext, HttpContextAuthorizationContext>();

// 2. Define policies (also just once)
services.AddAuthorization(options =>
{
    options.AddPolicy("DeleteUserCommand", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("GetReportQuery", policy =>
        policy.RequireRole("Manager", "Admin"));
    
    // Policies are checked automatically based on request's [Authorize] attribute
});

// 3. All your requests work automatically!
```

### 3. Register Authorization Context

For ASP.NET Core (register ONCE):
**No manual user passing needed!** It's all automatic.

## Custom Authorization Context (If Needed)

Only create a custom implementation if you have special requirements:

```csharp
using Waseet.CQRS.Authorization;
using System.Security.Claims;

public class MyAuthorizationContext : IAuthorizationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authService;

    public MyAuthorizationContext(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authService = authService;
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public async Task<bool> HasPolicyAsync(string policyName)
    {
        if (User == null) return false;
        
        var result = await _authService.AuthorizeAsync(User, policyName);
        return result.Succeeded;
    }

    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) ?? false;
    }
}
```

### 3. Register Authorization Context

```csharp
services.AddWaseetCQRS(typeof(Program).Assembly);

// Register your authorization context
services.AddScoped<IAuthorizationContext, MyAuthorizationContext>();

// Or use the default implementation for testing
services.AddScoped<IAuthorizationContext>(_ => 
    new DefaultAuthorizationContext(currentUser, "Policy1", "Policy2"));
```

## Authorization Behavior

The `AuthorizationBehavior` runs in the pipeline **before** validation and request handling:

```
Request → Authorization → Validation → Handler → Response
```

### Authorization Check Order

1. **Authentication Check**: Verifies user is authenticated
2. **Policy Check**: Validates user has required policies
3. **Role Check**: Confirms user has required roles
4. **All Checks Pass**: Proceeds to next pipeline behavior

## ASP.NET Core Integration

### Complete Setup Example

```csharp
using Microsoft.AspNetCore.Authorization;
using Waseet.CQRS;
using Waseet.CQRS.Authorization;
using Waseet.CQRS.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add ASP.NET Core authentication and authorization
builder.Services.AddAuthentication()
    .AddJwtBearer(options => { /* JWT config */ });

builder.Services.AddAuthorization(options =>
{
    // Define policies that match your command/query names
    options.AddPolicy("DeleteUserCommand", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("GetSensitiveDataQuery", policy =>
        policy.RequireClaim("DataAccess", "Sensitive"));
});

// Add Waseet.CQRS
builder.Services.AddWaseetCQRS(typeof(Program).Assembly);

// Register authorization context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationContext, AspNetCoreAuthorizationContext>();

var app = builder.Build();
```

### ASP.NET Core Authorization Context

```csharp
public class AspNetCoreAuthorizationContext : IAuthorizationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public AspNetCoreAuthorizationContext(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public async Task<bool> HasPolicyAsync(string policyName)
    {
        if (User == null) return false;
        
        var result = await _authorizationService.AuthorizeAsync(User, policyName);
        return result.Succeeded;
    }

    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) ?? false;
    }
}
```

## Error Handling

### AuthorizationException

When authorization fails, an `AuthorizationException` is thrown:

```csharp
try
{
    await mediator.Send(new DeleteUserCommand(userId));
}
catch (AuthorizationException ex)
{
    // ex.RequestName - The request that failed authorization
    // ex.PolicyName - The policy that was required
    // ex.RequiredRoles - The roles that were required
    // ex.WasAuthenticated - Whether the user was authenticated
    
    Console.WriteLine(ex.Message);
    // "User does not have permission to execute 'DeleteUserCommand'. 
    //  Required policy: 'CanDeleteUsers'."
}
```

### Error Messages

- **Not Authenticated**: "User is not authenticated. Request 'DeleteUserCommand' requires authentication."
- **Policy Failed**: "User does not have permission to execute 'DeleteUserCommand'. Required policy: 'CanDeleteUsers'."
- **Role Failed**: "User does not have permission to execute 'DeleteUserCommand'. Required roles: Admin, Manager."

## Authorization Patterns

### Pattern 1: Policy Matches Request Name

The simplest pattern - the policy name automatically matches the request name:

```csharp
[Authorize] // Checks for "CreateUserCommand" policy
public record CreateUserCommand(string Name, string Email) : IRequest<Guid>;

// In Startup.cs
services.AddAuthorization(options =>
{
    options.AddPolicy("CreateUserCommand", policy => 
        policy.RequireRole("UserManager"));
});
```

### Pattern 2: Explicit Policy Names

Use specific policy names for shared authorization logic:

```csharp
[Authorize(Policy = "CanManageUsers")]
public record CreateUserCommand(...) : IRequest<Guid>;

[Authorize(Policy = "CanManageUsers")]
public record UpdateUserCommand(...) : IRequest<Unit>;

[Authorize(Policy = "CanManageUsers")]
public record DeleteUserCommand(...) : IRequest<Unit>;
```

### Pattern 3: Role-Based Authorization

Simple role checks without policies:

```csharp
[Authorize(Roles = "Admin")]
public record DeleteAllDataCommand : IRequest<Unit>;

[Authorize(Roles = "Admin,Manager")] // Any of these roles
public record ViewReportsQuery : IRequest<Report>;
```

### Pattern 4: Combined Authorization

Multiple requirements - user must satisfy all:

```csharp
[Authorize(Policy = "CanAccessFinancial")]
[Authorize(Roles = "Accountant,Manager")]
public record GetFinancialReportQuery : IRequest<FinancialReport>;
```

## Testing with Authorization

### Using DefaultAuthorizationContext

For testing, use the default implementation:

```csharp
[Fact]
public async Task DeleteUser_RequiresAdminRole()
{
    // Arrange
    var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, "admin"),
        new Claim(ClaimTypes.Role, "Admin")
    }, "TestAuth"));

    var authContext = new DefaultAuthorizationContext(
        adminUser, 
        "DeleteUserCommand");

    var services = new ServiceCollection();
    services.AddScoped<IAuthorizationContext>(_ => authContext);
    services.AddWaseetCQRS(typeof(DeleteUserCommand).Assembly);
    
    var mediator = services.BuildServiceProvider()
        .GetRequiredService<IMediator>();

    // Act & Assert
    await mediator.Send(new DeleteUserCommand(Guid.NewGuid())); // Should succeed
}

[Fact]
public async Task DeleteUser_ThrowsWhenNotAuthorized()
{
    // Arrange - user without Admin role
    var regularUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, "user"),
        new Claim(ClaimTypes.Role, "User")
    }, "TestAuth"));

    var authContext = new DefaultAuthorizationContext(regularUser);

    var services = new ServiceCollection();
    services.AddScoped<IAuthorizationContext>(_ => authContext);
    services.AddWaseetCQRS(typeof(DeleteUserCommand).Assembly);
    
    var mediator = services.BuildServiceProvider()
        .GetRequiredService<IMediator>();

    // Act & Assert
    await Assert.ThrowsAsync<AuthorizationException>(
        () => mediator.Send(new DeleteUserCommand(Guid.NewGuid())));
}
```

## Best Practices

1. **Policy Names**: Use descriptive policy names that match your domain
2. **Request Names as Policies**: Leverage automatic policy matching for simplicity
3. **Granular Policies**: Create specific policies for different operations
4. **Role Hierarchy**: Use roles for broad access levels, policies for specific permissions
5. **Error Handling**: Always catch `AuthorizationException` in your API layer
6. **Testing**: Test both authorized and unauthorized scenarios
7. **Documentation**: Document which policies/roles are required for each endpoint

## Pipeline Order

Authorization runs **first** in the pipeline, before validation:

```csharp
// Order in ServiceCollectionExtensions
services.AddScoped(typeof(IPipelineBehavior<,>), 
    typeof(Authorization.AuthorizationBehavior<,>)); // First

services.AddScoped(typeof(IPipelineBehavior<,>), 
    typeof(Validation.ValidationBehavior<,>));       // Second
```

This ensures:
- Unauthorized users don't trigger validation errors
- Performance: Early exit for unauthorized requests
- Security: Authorization happens before any processing

## Comparison with Other Libraries

| Feature | Waseet.CQRS | MediatR + Custom | ASP.NET Core |
|---------|-------------|------------------|--------------|
| Built-in Authorization | ✅ Yes | ❌ Manual | ✅ Yes |
| Policy-Based | ✅ Yes | ⚠️ Custom | ✅ Yes |
| Role-Based | ✅ Yes | ⚠️ Custom | ✅ Yes |
| Attribute-Based | ✅ Yes | ❌ No | ✅ Yes |
| Pipeline Integration | ✅ Automatic | ⚠️ Manual | N/A |
| Request-Level | ✅ Yes | ⚠️ Custom | ❌ Controller-Level |

## See Also

- [Validation Documentation](VALIDATION.md)
- [Pipeline Behaviors](FEATURES.md#pipeline-behaviors)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
