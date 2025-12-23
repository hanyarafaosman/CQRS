# Project Summary: Waseet.CQRS (وسيط)

## Overview
**Waseet.CQRS** (وسيط - "Mediator" in Arabic) is a lightweight, high-performance mediator library for .NET with Arab identity, created to empower Arabic developers. It implements the Mediator pattern to support Command Query Responsibility Segregation (CQRS) architecture. It serves as a feature-complete alternative to MediatR with minimal dependencies and built-in validation, events, and streaming support.

Built by Arab developers, for the world 🌍

## Project Structure

```
Waseet.CQRS/
├── Waseet.CQRS.sln                      # Solution file
├── README.md                             # Main documentation
├── FEATURES.md                           # Detailed feature documentation
├── PACKAGE.md                            # NuGet packaging guide
├── .gitignore                            # Git ignore rules
│
├── src/
│   └── Waseet.CQRS/                     # Main library project
│       ├── Waseet.CQRS.csproj           # Project file with NuGet metadata
│       ├── IMediator.cs                 # Mediator interface (Send, CreateStream)
│       ├── IPublisher.cs                # Publisher interface (Publish)
│       ├── IRequest.cs                  # Request marker interfaces
│       ├── IRequestHandler.cs           # Handler interfaces
│       ├── INotification.cs             # Notification marker interface
│       ├── INotificationHandler.cs      # Notification handler interface
│       ├── IStreamRequest.cs            # Stream request marker interface
│       ├── IStreamRequestHandler.cs     # Stream handler interface
│       ├── IPipelineBehavior.cs         # Pipeline behavior interface
└── tests/
    └── Waseet.CQRS.Sample/              # Sample/test project
        ├── Program.cs                    # Demo application
        ├── Models/
        │   └── User.cs                   # Domain model
        ├── Commands/
        │   ├── CreateUserCommand.cs      # Command definition
        │   ├── CreateUserCommandHandler.cs
        │   ├── UpdateUserCommand.cs
        │   └── UpdateUserCommandHandler.cs
        ├── Queries/
        │   ├── GetUserQuery.cs           # Query definition
        │   ├── GetUserQueryHandler.cs
        │   ├── GetAllUsersQuery.cs
        │   ├── GetAllUsersQueryHandler.cs
        │   ├── StreamAllUsersQuery.cs    # Stream query
        │   ├── StreamAllUsersQueryHandler.cs
        │   ├── GenerateNumbersQuery.cs   # Stream query
        │   └── GenerateNumbersQueryHandler.cs
        ├── Events/
        │   ├── UserCreatedEvent.cs       # Event definition
        │   ├── UserCreatedLoggingHandler.cs
        │   ├── UserCreatedEmailHandler.cs
        │   ├── UserUpdatedEvent.cs
        │   └── UserUpdatedLoggingHandler.cs
        ├── Validators/
        │   └── CreateUserCommandValidator.cs
        └── Data/
            └── UserRepository.cs         # In-memory repository
        │   ├── CreateUserCommand.cs      # Command definition
        │   ├── CreateUserCommandHandler.cs
        │   ├── UpdateUserCommand.cs
### 1. Library (Waseet.CQRS)
- **20+ core files** implementing the mediator pattern
- **1 dependency**: Microsoft.Extensions.DependencyInjection.Abstractions
- **800+ lines** of well-documented code
- **XML documentation** for IntelliSense support
- **NuGet package ready** with proper metadata

### 2. Sample Application (CQRS.Mediator.Sample)
- **20+ files** demonstrating real-world usage
- Complete CRUD operations example
- Commands with and without responses
- Query implementations (sync and streaming)
- Event publishing with multiple handlers
- Validation with error handling
- Dependency injection setupr)
- **6 core files** implementing the mediator pattern
- **1 dependency**: Microsoft.Extensions.DependencyInjection.Abstractions
- **300+ lines** of well-documented code
- **XML documentation** for IntelliSense support
- **NuGet package ready** with proper metadata

### 2. Sample Application (CQRS.Mediator.Sample)
- **10 files** demonstrating real-world usage
- Complete CRUD operations example
- Commands with and without responses
- Query implementations
## Features Implemented ✅

1. **Request/Response Pattern**
   - Generic request interface `IRequest<TResponse>`
   - Support for void operations via `Unit` type
   - Type-safe requests and responses

2. **Handler System**
   - `IRequestHandler<TRequest, TResponse>` interface
   - Async/await support
   - Cancellation token propagation

3. **Mediator Implementation**
   - Dynamic handler resolution
   - Reflection-based invocation
   - Comprehensive error handling

4. **Pipeline Behaviors**
   - `IPipelineBehavior<TRequest, TResponse>` interface
   - Full pipeline execution with delegate composition
   - Support for cross-cutting concerns (logging, validation, etc.)

5. **Validation System**
   - `IValidator<TRequest>` interface
   - `ValidationBehavior` pipeline implementation
   - `ValidationResult` with multiple error support
   - `ValidationException` for automatic error handling
   - Parallel validation execution

6. **Event-Driven Architecture (Pub/Sub)**
   - `INotification` marker interface
   - `INotificationHandler<TNotification>` interface
   - `IPublisher` with `Publish` method
   - Parallel execution of multiple handlers
   - Support for multiple handlers per notification

7. **Stream Support**
   - `IStreamRequest<TResponse>` interface
   - `IStreamRequestHandler<TRequest, TResponse>` interface
   - `IAsyncEnumerable<T>` return type
   - `CreateStream` method in mediator
   - Pipeline behaviors for streaming (`IStreamPipelineBehavior`)
   - Efficient processing of large datasets

8. **Dependency Injection**
   - Assembly scanning for handlers, validators, and notification handlers
   - Automatic registration
   - Fluent configuration API
   - Microsoft.Extensions.DependencyInjection integration

9. **Documentation**
   - XML documentation comments
   - Comprehensive README
   - Feature comparison document (FEATURES.md)
   - Validation guide (VALIDATION.md)
   - Events guide (EVENTS.md)
   - Streaming guide (STREAMING.md)
   - NuGet packaging guide (PACKAGE.md)
   - Sample application with 8+ examples

## Features Comparison with MediatR

| Feature | Waseet.CQRS | MediatR |
|---------|---------------|---------|
| Request/Response | ✅ | ✅ |
| Pipeline Behaviors | ✅ | ✅ |
| Notifications/Events | ✅ | ✅ |
| Stream Support | ✅ | ✅ |
| Built-in Validation | ✅ | ❌ (requires FluentValidation) |
| Polymorphic Dispatch | ❌ | ✅ |

## Future Enhancements 🚧

1. **Performance Optimizations**
   - Compiled expressions instead of reflection
   - Handler type caching
   - Source generators for compile-time registration

2. **Advanced Features**
   - Polymorphic dispatch (covariance support)
   - Request/response caching
   - Circuit breaker pattern
   - Automatic retry logic
   - Performance metrics and monitoring
3. **Streaming Support**
4. **Performance Optimizations** (compiled expressions, caching)

## Quick Start

### Installation
```bash
# Build the library
cd src\CQRS.Mediator
dotnet build

# Create NuGet package
dotnet pack -c Release
```

### Usage
```csharp
// 1. Define a request
public record GetUserQuery(Guid UserId) : IRequest<User>;

// 2. Implement handler
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken ct)
    {
        // Your logic here
        return user;
    }
}

// 3. Register services
services.AddMediator(typeof(Program).Assembly);

// 4. Use mediator
var user = await mediator.Send(new GetUserQuery(userId));
```

## Running the Sample

```bash
cd tests\CQRS.Mediator.Sample
dotnet run
```

**Expected Output:**
```
=== CQRS Mediator Demo ===

1. Creating a new user...
   User created with ID: <guid>

2. Updating user...
   User updated successfully

3. Getting user details...
   User Details: ID=<guid>, Name=John Smith, Email=john.doe@example.com

4. Getting all users...
   Total users: 1
   - John Smith (john.doe@example.com)

=== Demo completed successfully! ===
```

## Building the Project

```bash
# Build entire solution
dotnet build CQRS.Mediator.sln

# Build library only
cd src\CQRS.Mediator
dotnet build

# Build sample
cd tests\CQRS.Mediator.Sample
dotnet build

# Create NuGet package
cd src\CQRS.Mediator
dotnet pack -c Release
```

## NuGet Package

**Package Name:** CQRS.Mediator  
**Version:** 1.0.0  
**Target Framework:** .NET 10.0  
**Dependencies:** Microsoft.Extensions.DependencyInjection.Abstractions 10.0.1

**Package Location:**  
`src\CQRS.Mediator\bin\Release\CQRS.Mediator.1.0.0.nupkg`

## Testing

The sample project serves as both documentation and testing:
- ✅ Command with response (CreateUserCommand → Guid)
- ✅ Command without response (UpdateUserCommand → Unit)
- ✅ Query (GetUserQuery → User)
- ✅ List query (GetAllUsersQuery → List<User>)
- ✅ Dependency injection setup
- ✅ Handler resolution
- ✅ Repository pattern integration

## Comparison to MediatR

### Advantages
- ✅ **Simpler**: Fewer concepts to learn
- ✅ **Lighter**: Minimal dependencies
- ✅ **Transparent**: Easy to understand the code
- ✅ **Smaller**: ~300 lines vs thousands

### Trade-offs
- ❌ No pipeline behaviors execution
- ❌ No notification system
- ❌ No streaming support
- ❌ Basic assembly scanning

## Use Cases

**Ideal for:**
- Learning CQRS patterns
- Small to medium applications
- Projects preferring minimal dependencies
- Teams wanting full control over the code

**Consider MediatR for:**
- Large enterprise applications
- Advanced pipeline behavior needs
- Event/notification systems
- Streaming requirements

## Technical Details

**Language:** C# 13  
**Framework:** .NET 10.0  
**Pattern:** Mediator + CQRS  
**Architecture:** Clean Architecture compatible  
**Testing:** Console application demo  
**Documentation:** XML comments + Markdown

## Next Steps

1. **Add Unit Tests**: Create xUnit/NUnit test project
2. **Implement Pipeline Behaviors**: Wire up the existing interface
3. **Add Benchmarks**: Compare performance with MediatR
4. **Publish to NuGet**: Make publicly available
5. **Add Examples**: API, Blazor, Worker Service demos
6. **Performance**: Replace reflection with compiled expressions
7. **Notifications**: Add event/notification support

## License

This is a demonstration/sample project. Intended for educational purposes and as a starting point for custom implementations.

## Contributing

This is a complete, working implementation that can be:
- Extended with additional features
- Optimized for performance
- Integrated into larger projects
- Used as a learning resource

---

**Status:** ✅ Fully functional and ready to use  
**Build:** ✅ Passing  
**Documentation:** ✅ Complete  
**Sample:** ✅ Working  
**NuGet:** ✅ Package created
