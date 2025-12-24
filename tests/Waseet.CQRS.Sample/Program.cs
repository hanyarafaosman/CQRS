using Waseet.CQRS;
using Waseet.CQRS.Authorization;
using Waseet.CQRS.Extensions;
using Waseet.CQRS.Sample.Commands;
using Waseet.CQRS.Sample.Data;
using Waseet.CQRS.Sample.Queries;
using Waseet.CQRS.Validation;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

// Setup dependency injection
var services = new ServiceCollection();

// Register repository
services.AddSingleton<UserRepository>();

// Register Waseet CQRS and handlers from this assembly
services.AddWaseet(typeof(Program).Assembly);

// Add validation support
services.AddWaseetValidation(typeof(Program).Assembly);

// ✨ Add new features: Caching, Monitoring, Auditing, and Idempotency
services.AddWaseetCaching();  // Uses in-memory cache
services.AddWaseetMonitoring(); // Tracks performance
services.AddWaseetAuditing(); // Tracks changes
services.AddWaseetIdempotency(); // Prevents duplicate commands
// services.AddWaseetElasticsearchAuditing(
//     "https://cashing:9200",
//     indexPrefix: "waseet-audit",
//     username: "elastic",
//     password: "passw@red",
//     ignoreSslErrors: true  // Ignore SSL errors for self-signed certificates
// );

// For Elasticsearch (comment out console logger above and uncomment this):
// services.AddWaseetElasticsearchAuditing("http://localhost:9200", "waseet-audit");

// Register authorization context with sample user and policies
var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.Name, "admin@example.com"),
    new Claim(ClaimTypes.Role, "Admin")
}, "TestAuth"));

services.AddScoped<IAuthorizationContext>(_ => 
    new DefaultAuthorizationContext(adminUser, "GetSensitiveDataQuery", "DeleteUserCommand"));


// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get mediator instance
var mediator = serviceProvider.GetRequiredService<IMediator>();

Console.WriteLine("=== Waseet.CQRS Demo (Caching, Monitoring & Auditing) ===\n");

// Example 1: Create User Command with validation
Console.WriteLine("1. Creating a new user...");
Guid firstUserId = Guid.Empty;
try
{
    var createUserCommand = new CreateUserCommand("John Doe", "john.doe@example.com");
    firstUserId = await mediator.Send(createUserCommand);
    Console.WriteLine($"   User created with ID: {firstUserId}");
}
catch (ValidationException ex)
{
    Console.WriteLine($"   Validation failed: {ex.Message}");
}

Console.WriteLine();

// Example 2: Try creating user with invalid data (validation should fail)
Console.WriteLine("2. Attempting to create user with invalid data...");
try
{
    var invalidCommand = new CreateUserCommand("", "invalid-email");
    var userId = await mediator.Send(invalidCommand);
    Console.WriteLine($"   User created with ID: {userId}");
}
catch (ValidationException ex)
{
    Console.WriteLine($"   ✓ Validation caught errors:");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"     - {error.PropertyName}: {error.ErrorMessage}");
    }
}

Console.WriteLine();

// Example 3: Create another valid user
Console.WriteLine("3. Creating another user...");
var createUserCommand2 = new CreateUserCommand("Jane Smith", "jane.smith@example.com");
var userId2 = await mediator.Send(createUserCommand2);
Console.WriteLine($"   User created with ID: {userId2}");
Console.WriteLine();

// Example 4: Update User Command (triggers event)
Console.WriteLine("4. Updating user...");
var updateUserCommand = new UpdateUserCommand(userId2, "Jane Doe");
await mediator.Send(updateUserCommand);
Console.WriteLine("   User updated successfully");
Console.WriteLine();

// Example 5: Get User Query
Console.WriteLine("5. Getting user details...");
var getUserQuery = new GetUserQuery(userId2);
var user = await mediator.Send(getUserQuery);
Console.WriteLine($"   User Details: ID={user.Id}, Name={user.Name}, Email={user.Email}");
Console.WriteLine();

// Example 6: Get All Users Query
Console.WriteLine("6. Getting all users...");
var getAllUsersQuery = new GetAllUsersQuery();
var users = await mediator.Send(getAllUsersQuery);
Console.WriteLine($"   Total users: {users.Count}");
foreach (var u in users)
{
    Console.WriteLine($"   - {u.Name} ({u.Email})");
}

Console.WriteLine("\n=== Demo completed successfully! ===");
Console.WriteLine("\nFeatures demonstrated:");
Console.WriteLine("✓ Request/Response pattern (Commands & Queries)");
Console.WriteLine("✓ Validation with pipeline behaviors");
Console.WriteLine("✓ Event-driven architecture (Notifications)");
Console.WriteLine("✓ Multiple event handlers per event");
Console.WriteLine("✓ Dependency injection integration");

// Example 7: Authorization - Access with permission
Console.WriteLine("\n7. Accessing sensitive data (with permission)...");
try
{
    var sensitiveData = await mediator.Send(new GetSensitiveDataQuery());
    Console.WriteLine($"   {sensitiveData}");
    Console.WriteLine("   ✓ Authorization check passed");
}
catch (AuthorizationException ex)
{
    Console.WriteLine($"   ✗ Authorization failed: {ex.Message}");
}

// Example 8: Authorization - Delete user with Admin role (with auditing)
Console.WriteLine("\n8. Attempting to delete user (requires Admin role, audited)...");
try
{
    await mediator.Send(new DeleteUserAuditedCommand(firstUserId));
    Console.WriteLine("   ✓ Delete authorized and audited for Admin role");
}
catch (AuthorizationException ex)
{
    Console.WriteLine($"   ✗ Authorization failed: {ex.Message}");
}

// Example 9: Caching - Get user by ID (first time - cache miss)
Console.WriteLine("\n9. Getting user by ID (first time - cache miss)...");
var userById1 = await mediator.Send(new GetUserByIdQuery(userId2));
Console.WriteLine($"   ✓ Retrieved: {userById1?.Name}");

// Example 10: Caching - Get user by ID (second time - cache hit!)
Console.WriteLine("\n10. Getting user by ID again (cache hit!)...");
var userById2 = await mediator.Send(new GetUserByIdQuery(userId2));
Console.WriteLine($"   ✓ Retrieved from cache: {userById2?.Name}");

// Example 11: Cache invalidation - Update user
Console.WriteLine("\n11. Updating user (invalidates cache)...");
await mediator.Send(new UpdateUserCommand(userId2, "Jane Smith Updated"));
Console.WriteLine("   ✓ User updated and cache invalidated");

// Example 12: Verify cache was invalidated
Console.WriteLine("\n12. Getting user again (cache was cleared)...");
var userById3 = await mediator.Send(new GetUserByIdQuery(userId2));
Console.WriteLine($"   ✓ Retrieved from database: {userById3?.Name}");

// Example 13: Performance monitoring - Get all users
Console.WriteLine("\n13. Getting all users (monitored for performance)...");
var allUsers = await mediator.Send(new GetAllUsersQuery());
Console.WriteLine($"   ✓ Retrieved {allUsers.Count} users");

// Example 14: Stream Support - Generate numbers
Console.WriteLine("\n14. Streaming numbers (1-5)...");
await foreach (var number in mediator.CreateStream(new GenerateNumbersQuery(5)))
{
    Console.WriteLine($"   Received: {number}");
}

// Example 15: Stream Support - Stream users
Console.WriteLine("\n15. Streaming all users...");
await foreach (var streamedUser in mediator.CreateStream(new StreamAllUsersQuery()))
{
    Console.WriteLine($"   Streamed user: {streamedUser.Name} ({streamedUser.Email})");
}

// Example 16: Show performance statistics
Console.WriteLine("\n16. Performance Statistics:");
var monitor = serviceProvider.GetRequiredService<Waseet.CQRS.Monitoring.IPerformanceMonitor>();
var stats = await monitor.GetStatisticsAsync();
Console.WriteLine($"   Total Requests: {stats.TotalRequests}");
Console.WriteLine($"   Successful: {stats.SuccessfulRequests}");
Console.WriteLine($"   Failed: {stats.FailedRequests}");
Console.WriteLine($"   Average Duration: {stats.AverageDurationMs:F2}ms");
Console.WriteLine($"   Min Duration: {stats.MinDurationMs}ms");
Console.WriteLine($"   Max Duration: {stats.MaxDurationMs}ms");
Console.WriteLine($"   Slow Requests: {stats.SlowRequestCount}");
Console.WriteLine($"   Success Rate: {stats.SuccessRate:F2}%");

Console.WriteLine("\n=== All features demonstrated! ===");
Console.WriteLine("\nFeatures demonstrated:");
Console.WriteLine("✓ Request/Response pattern (Commands & Queries)");
Console.WriteLine("✓ Response Caching with automatic invalidation");
Console.WriteLine("✓ Performance Monitoring with statistics");
Console.WriteLine("✓ Audit Logging (console/Elasticsearch)");
Console.WriteLine("✓ Idempotency (prevents duplicate commands)");
Console.WriteLine("✓ Validation with pipeline behaviors");
Console.WriteLine("✓ Authorization with policies and roles");
Console.WriteLine("✓ Event-driven architecture (Notifications)");
Console.WriteLine("✓ Multiple event handlers per event");
Console.WriteLine("✓ Dependency injection integration");
Console.WriteLine("✓ Stream support with IAsyncEnumerable");

// Example 17: Idempotency - Create payment (first time)
Console.WriteLine("\n17. Creating payment with idempotency key...");
var idempotencyKey = Guid.NewGuid().ToString();
var payment1 = await mediator.Send(new CreatePaymentCommand(idempotencyKey, 99.99m, "Premium subscription"));
Console.WriteLine($"   ✓ Payment created with ID: {payment1}");

// Example 18: Idempotency - Duplicate request (should return cached result)
Console.WriteLine("\n18. Attempting duplicate payment (same idempotency key)...");
var payment2 = await mediator.Send(new CreatePaymentCommand(idempotencyKey, 99.99m, "Premium subscription"));
Console.WriteLine($"   ✓ Duplicate prevented! Returned cached payment ID: {payment2}");
Console.WriteLine($"   ✓ Same ID returned: {payment1 == payment2}");

Console.WriteLine("\n=== Idempotency demonstration complete! ===");
