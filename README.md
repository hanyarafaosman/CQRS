# Waseet.CQRS (وسيط)

**Waseet** (وسيط - "Mediator" in Arabic) is a lightweight, high-performance mediator library for .NET with Arab identity. Built to empower Arabic developers and serve the global community, Waseet implements the Mediator pattern to support CQRS (Command Query Responsibility Segregation) architecture with built-in validation, authorization, events, streaming, caching, performance monitoring, and audit logging.

Created by Arab developers, for the world 🌍

## Features

- **Simple API**: Easy-to-use interface similar to MediatR
- **Request/Response Pattern**: Support for both commands (with/without response) and queries
- **Event-Driven Architecture**: Publish notifications to multiple handlers (pub/sub pattern)
- **Validation**: Built-in validation pipeline behavior with automatic error handling
- **Authorization**: Policy-based and role-based authorization with pipeline integration
- **Response Caching**: Attribute-based caching with automatic invalidation
- **Performance Monitoring**: Built-in request performance tracking and statistics
- **Audit Logging**: Automatic audit logging with Elasticsearch support
- **Pipeline Behaviors**: Support for cross-cutting concerns with ordered execution
- **Stream Support**: Process large datasets efficiently with `IAsyncEnumerable<T>`
- **Dependency Injection**: Built-in support for Microsoft.Extensions.DependencyInjection
- **Automatic Handler Registration**: Scan assemblies to automatically register handlers
- **Lightweight**: Minimal dependencies and overhead
- **Type-Safe**: Strongly typed requests, responses, events, and streams

## Installation

```bash
dotnet add package Waseet.CQRS
```

Or clone and build from source:

```bash
git clone https://github.com/yourusername/waseet-cqrs.git
cd waseet-cqrs
dotnet build
```

## Quick Start

### 1. Define a Request

```csharp
using Waseet.CQRS;

// Query that returns a response
public record GetUserQuery(Guid UserId) : IRequest<User>;

// Command that returns a response
public record CreateUserCommand(string Name, string Email) : IRequest<Guid>;

// Command with no response
public record UpdateUserCommand(Guid UserId, string NewName) : IRequest;
```

### 2. Create a Handler

```csharp
using Waseet.CQRS;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly IUserRepository _repository;

    public GetUserQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.UserId);
    }
}
```

### 3. Register Services

```csharp
using Waseet.CQRS.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register Waseet CQRS and all handlers from the specified assembly
services.AddWaseet(typeof(Program).Assembly);

// Add optional features
services.AddWaseetValidation(typeof(Program).Assembly);
services.AddWaseetCaching();
services.AddWaseetMonitoring();
services.AddWaseetAuditing();
// Or use Elasticsearch for audit logs
// services.AddWaseetElasticsearchAuditing("http://localhost:9200", "waseet-audit");

// Or use configuration style
services.AddWaseet(config =>
{
    config.RegisterServicesFromAssemblyContaining<Program>();
});
```

### 4. Send Requests

```csharp
var mediator = serviceProvider.GetRequiredService<IMediator>();

// Send a query
var user = await mediator.Send(new GetUserQuery(userId));

// Send a command with response
var newUserId = await mediator.Send(new CreateUserCommand("John Doe", "john@example.com"));

// Send a command without response
await mediator.Send(new UpdateUserCommand(userId, "Jane Doe"));
```

## Advanced Features

### Response Caching

Cache query responses automatically:

```csharp
[Cache(Key = "user-{UserId}", Duration = 300)] // Cache for 5 minutes
public record GetUserByIdQuery(Guid UserId) : IRequest<User>;

[InvalidateCache("user-{UserId}")] // Clear cache on update
public record UpdateUserCommand(Guid UserId, string Name) : IRequest;
```

### Authorization

Secure commands and queries with policy or role-based authorization:

```csharp
[Authorize(Roles = "Admin")]
public record DeleteUserCommand(Guid UserId) : IRequest;

[Authorize(Policy = "SensitiveDataAccess")]
public record GetSensitiveDataQuery : IRequest<string>;

// Register authorization context
services.AddScoped<IAuthorizationContext, YourAuthContextImplementation>();
```

### Performance Monitoring

Track request performance automatically:

```csharp
[Monitor(SlowThresholdMs = 100)]
public record GetAllUsersQuery : IRequest<List<User>>;

// Get statistics
var monitor = serviceProvider.GetRequiredService<IPerformanceMonitor>();
var stats = await monitor.GetStatisticsAsync();
Console.WriteLine($"Average Duration: {stats.AverageDurationMs}ms");
```

### Audit Logging

Automatically log operations to Elasticsearch:

```csharp
[Audit(IncludeRequest = true, Category = "UserManagement")]
public record CreateUserCommand(string Name, string Email) : IRequest<Guid>;

// Logs include: timestamp, user, request/response data, duration, success status
```

### Validation

Define validators for your requests:

```csharp
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public ValidationResult Validate(CreateUserCommand request)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "Name is required"));

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add(new ValidationError(nameof(request.Email), "Email is required"));

        return errors.Count > 0 
            ? ValidationResult.Failure(errors.ToArray()) 
            : ValidationResult.Success();
    }
}
```

### Handle Validation Errors

```csharp
try
{
    await mediator.Send(new CreateUserCommand("", "invalid"));
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

## Event-Driven Architecture

### Define an Event

```csharp
public record UserCreatedEvent(Guid UserId, string Name, string Email) : INotification;
```

### Create Event Handlers

```csharp
public class UserCreatedLoggingHandler : INotificationHandler<UserCreatedEvent>
{
    public Task Handle(UserCreatedEvent notification, CancellationToken ct)
    {
        Console.WriteLine($"User created: {notification.Name}");
        return Task.CompletedTask;
    }
}

public class UserCreatedEmailHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken ct)
    {
        await _emailService.SendWelcomeEmailAsync(notification.Email);
    }
}
```

### Publish Events

```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IPublisher _publisher;
    
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Create user...
        
        // Publish event - all handlers will be notified
        await _publisher.Publish(new UserCreatedEvent(userId, name, email), ct);
        
        return userId;
    }
}
```

## Key Interfaces

### IRequest<TResponse>
Marker interface for requests that return a response.

### IRequest
Marker interface for requests that don't return a response (returns Unit).

### IRequestHandler<TRequest, TResponse>
Defines a handler for a request.

### IMediator
Defines the mediator to send requests to handlers.

### IPipelineBehavior<TRequest, TResponse>
Interface for implementing cross-cutting concerns (not yet fully implemented).

## Unit Type

The library includes a `Unit` type to represent void operations:

```csharp
public record DeleteUserCommand(Guid UserId) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    public Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Perform deletion
        return Task.FromResult(Unit.Value);
    }
}
```

## Sample Project

Check the `tests/Waseet.CQRS.Sample` project for a complete working example with:
- Commands and Queries
- Handler implementations
- Dependency injection setup
- Validation with validators
- Authorization with policies and roles
- Response caching with invalidation
- Performance monitoring
- Audit logging
- Event-driven architecture with notifications
- Stream support for large datasets

## Documentation

- **[README.md](README.md)** - This file, quick start guide
- **[FEATURES.md](FEATURES.md)** - Detailed feature documentation and architecture
- **[VALIDATION.md](VALIDATION.md)** - Complete validation guide with examples
- **[AUTHORIZATION.md](AUTHORIZATION.md)** - Authorization patterns and best practices
- **[EVENTS.md](EVENTS.md)** - Event-driven architecture patterns
- **[STREAMING.md](STREAMING.md)** - Stream support guide for large datasets
- **[PACKAGE.md](PACKAGE.md)** - NuGet packaging and publishing guide
- **[PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)** - Complete project overview

## Comparison with MediatR

| Feature | Waseet.CQRS | MediatR |
|---------|-------------|---------|
| Request/Response | ✅ | ✅ |
| Automatic Handler Registration | ✅ | ✅ |
| Pipeline Behaviors | ✅ | ✅ |
| Validation | ✅ Built-in | ❌ Requires FluentValidation |
| Authorization | ✅ Built-in | ❌ Requires custom implementation |
| Response Caching | ✅ Built-in | ❌ Requires custom implementation |
| Performance Monitoring | ✅ Built-in | ❌ Requires custom implementation |
| Audit Logging | ✅ Built-in | ❌ Requires custom implementation |
| Elasticsearch Integration | ✅ Built-in | ❌ |
| Notifications/Events | ✅ | ✅ |
| Stream Support | ✅ | ✅ |
| Dependencies | Minimal | More comprehensive |
| Arab Identity | ✅ وسيط | ❌ |

## License

MIT License

Copyright (c) 2025 Waseet.CQRS Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Support

For issues, questions, or contributions, please visit our [GitHub repository](https://github.com/yourusername/waseet-cqrs).

---

Made with ❤️ by Arab developers, for developers worldwide 🌍
