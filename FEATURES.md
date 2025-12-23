# CQRS.Mediator Features & Architecture

## Core Components

### 1. **IRequest<TResponse>**
The marker interface for all requests that expect a response.

```csharp
public interface IRequest<out TResponse> { }
public interface IRequest : IRequest<Unit> { }
```

**Key Features:**
- Generic response type
- Support for void operations via `Unit` type
- Compile-time type safety

### 2. **IRequestHandler<TRequest, TResponse>**
The interface that all handlers must implement.

```csharp
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}
```

**Key Features:**
- Asynchronous by default
- Cancellation token support
- Strongly typed request/response

### 3. **IMediator**
The main interface for sending requests and creating streams.

```csharp
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, 
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request, 
        CancellationToken cancellationToken = default);
}
```

**Key Features:**
- Send method for single request/response
- CreateStream method for streaming responses
- Generic response handling
- Request dispatching to appropriate handlers

### 4. **Mediator Implementation**
The concrete implementation using reflection and dependency injection.

**Features:**
- ✅ Dynamic handler resolution via DI container
- ✅ Runtime type discovery
- ✅ Automatic handler invocation
- ✅ Comprehensive error handling

### 5. **Unit Type**
A struct representing void/no return value.

```csharp
public struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = new();
}
```

**Benefits:**
- Type-safe void operations
- No special handling needed
- Consistent API surface

## Dependency Injection Integration

### ServiceCollectionExtensions
Provides seamless integration with Microsoft.Extensions.DependencyInjection.

**Features:**
1. **Assembly Scanning**
   ```csharp
   services.AddMediator(typeof(Program).Assembly);
   ```

2. **Fluent Configuration**
   ```csharp
   services.AddMediator(config =>
   {
       config.RegisterServicesFromAssemblyContaining<Program>();
   });
   ```

3. **Automatic Handler Registration**
   - Scans assemblies for `IRequestHandler<,>` implementations
   - Registers handlers as `Transient` services
   - Registers `IMediator` as `Scoped` service

## Request/Response Patterns

### Pattern 1: Query (Read Operation)
```csharp
// Request
public record GetUserQuery(Guid UserId) : IRequest<User>;

// Handler
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    public Task<User> Handle(GetUserQuery request, CancellationToken ct)
    {
        // Query database
        return Task.FromResult(user);
    }
}

// Usage
var user = await mediator.Send(new GetUserQuery(userId));
```

### Pattern 2: Command with Response
```csharp
// Request
public record CreateUserCommand(string Name, string Email) : IRequest<Guid>;

// Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    public Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Create user and return ID
        return Task.FromResult(newUserId);
    }
}

// Usage
var newUserId = await mediator.Send(new CreateUserCommand("John", "john@example.com"));
```

### Pattern 3: Command without Response
```csharp
// Request
public record DeleteUserCommand(Guid UserId) : IRequest;

// Handler
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    public Task<Unit> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        // Delete user
        return Task.FromResult(Unit.Value);
    }
}

// Usage
await mediator.Send(new DeleteUserCommand(userId));
```

## Pipeline Behaviors (Interface Only)

The library includes `IPipelineBehavior<TRequest, TResponse>` interface for future extensibility:

```csharp
public interface IPipelineBehavior<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken);
}
```

**Potential Use Cases:**
- Logging
- Validation
- Caching
- Transaction management
- Performance monitoring
- Exception handling

*Note: Pipeline behavior execution is not yet implemented in the mediator.*

## Architecture Benefits

### 1. **Separation of Concerns**
- Clear separation between request definition and handling
- Business logic encapsulated in handlers
- UI/API layer decoupled from business logic

### 2. **Testability**
- Handlers can be tested in isolation
- Easy to mock `IMediator` interface
- No infrastructure dependencies required

### 3. **Single Responsibility**
- Each handler does one thing
- Easy to maintain and modify
- Clear code organization

### 4. **Open/Closed Principle**
- Add new requests/handlers without modifying existing code
- Extension through composition

### 5. **Dependency Inversion**
- Depend on abstractions (`IMediator`, `IRequest`)
- Implementation details hidden

## Performance Considerations

### Current Implementation
- **Handler Resolution**: Reflection-based (suitable for most applications)
- **Registration**: One-time assembly scanning at startup
- **Invocation**: Dynamic method invocation per request

### Optimization Opportunities
1. **Compiled Expressions**: Replace reflection with compiled expressions
2. **Handler Caching**: Cache resolved handler types
3. **Source Generators**: Generate handler mappings at compile time

## Comparison with MediatR

| Feature | CQRS.Mediator | MediatR |
|---------|---------------|---------|
| **Core Request/Response** | ✅ Fully implemented | ✅ |
| **Async/Await** | ✅ Built-in | ✅ |
| **Cancellation Tokens** | ✅ Supported | ✅ |
| **DI Integration** | ✅ Microsoft.Extensions.DI | ✅ Multiple containers |
| **Assembly Scanning** | ✅ Full support | ✅ Advanced |
| **Pipeline Behaviors** | ✅ Fully implemented | ✅ Full implementation |
| **Notifications/Events** | ✅ Fully implemented | ✅ Full support |
| **Streaming** | ✅ **Fully implemented** | ✅ IAsyncEnumerable |
| **Validation** | ✅ Built-in | ❌ Requires FluentValidation |
| **Pre/Post Processors** | ✅ Via Pipeline Behaviors | ✅ Supported |
| **Polymorphic Dispatch** | ❌ Not implemented | ✅ Covariance support |
| **Dependencies** | Minimal (1 package) | More extensive |
| **Learning Curve** | Simple | Moderate |
| **Code Size** | ~800 lines | Extensive |

## Extension Points

### Current Features
1. Custom mediator implementation (`IMediator`)
2. Custom handler registration logic
3. Pipeline behavior execution (validation, logging, etc.)
4. Notification/event system with parallel execution
5. Stream support with `IAsyncEnumerable<T>`
6. Built-in validation framework

### Future Possibilities
1. Polymorphic dispatch (covariance support)
2. Performance monitoring and metrics
3. Automatic retry logic
4. Circuit breaker pattern
5. Request caching strategies
## Usage Guidelines

### When to Use CQRS.Mediator
- ✅ Small to medium-sized applications
- ✅ Learning CQRS patterns
- ✅ Minimal dependencies preferred
- ✅ Need built-in validation without external libraries
- ✅ Event-driven architecture requirements
- ✅ Stream processing for large datasets
- ✅ ASP.NET Core applications

### When to Use MediatR Instead
- Need polymorphic dispatch (covariance)
- Existing large codebase already using MediatR
- Need specific MediatR ecosystem packages
- Require backward compatibility with older MediatR featuresired
- Streaming support needed
- Complex cross-cutting concerns
## Code Examples

See the `tests/CQRS.Mediator.Sample` project for complete working examples including:
- Command handling (Create, Update operations)
- Query handling (Get single, Get multiple)
- Event publishing and handling (multiple handlers per event)
- Validation with automatic error handling
- Stream processing with `IAsyncEnumerable<T>`
- Pipeline behaviors for cross-cutting concerns
See the `tests/CQRS.Mediator.Sample` project for complete working examples including:
- Multiple command types
- Query implementations
- DI setup and configuration
- Real-world usage patterns
