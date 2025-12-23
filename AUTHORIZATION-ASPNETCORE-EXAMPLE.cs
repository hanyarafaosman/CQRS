using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Waseet.CQRS.AspNetCore;
using Waseet.CQRS.Sample.Commands;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// STEP 1: Add Authentication (JWT, Cookie, etc.)
// ============================================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// ============================================================================
// STEP 2: Add Authorization with Policies
// ============================================================================
builder.Services.AddAuthorization(options =>
{
    // Define policies that match your command/query names
    // These are checked automatically when requests have [Authorize] attribute
    
    // Policy matches request name automatically
    options.AddPolicy("DeleteUserCommand", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("GetSensitiveDataQuery", policy =>
        policy.RequireClaim("DataAccess", "Sensitive"));

    // Or use custom policy
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole("Admin", "Manager"));
});

// ============================================================================
// STEP 3: Add Waseet.CQRS with ASP.NET Core Integration
// THIS IS THE ONLY LINE YOU NEED - Register ONCE, works for ALL requests!
// ============================================================================
builder.Services.AddWaseetCQRSWithAspNetCore(typeof(Program).Assembly);

// ============================================================================
// STEP 4: Add your other services
// ============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
app.UseAuthentication();  // Must be before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();

// ============================================================================
// HOW IT WORKS:
// ============================================================================
// 1. User makes HTTP request with JWT token
// 2. ASP.NET Core authenticates user from token
// 3. When you call mediator.Send():
//    - AuthorizationBehavior automatically gets current user from HttpContext
//    - Checks [Authorize] attributes on your request
//    - Validates against policies you defined above
//    - If authorized: proceeds to handler
//    - If not authorized: throws AuthorizationException
// 4. No need to pass user or context manually - it's all automatic!

// ============================================================================
// EXAMPLE CONTROLLER:
// ============================================================================
// [ApiController]
// [Route("api/[controller]")]
// public class UsersController : ControllerBase
// {
//     private readonly IMediator _mediator;
//
//     public UsersController(IMediator mediator)
//     {
//         _mediator = mediator;
//     }
//
//     [HttpDelete("{id}")]
//     public async Task<IActionResult> DeleteUser(Guid id)
//     {
//         try
//         {
//             // Authorization happens automatically!
//             // No need to pass user or check permissions manually
//             await _mediator.Send(new DeleteUserCommand(id));
//             return Ok();
//         }
//         catch (AuthorizationException ex)
//         {
//             return Forbid(); // Returns 403
//         }
//     }
// }
