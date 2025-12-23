# Event-Driven Architecture Guide

## Overview
The CQRS.Mediator library supports event-driven architecture through notifications. Events (notifications) are published to multiple handlers, enabling loose coupling and reactive programming.

## Quick Start

### 1. Define an Event

Events implement `INotification`:

```csharp
using CQRS.Mediator;

public record UserCreatedEvent(Guid UserId, string Name, string Email) : INotification;
```

### 2. Create Event Handlers

Create one or more handlers for your event:

```csharp
public class UserCreatedLoggingHandler : INotificationHandler<UserCreatedEvent>
{
    public Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"User created: {notification.Name}");
        return Task.CompletedTask;
    }
}

public class UserCreatedEmailHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;
    
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendWelcomeEmailAsync(notification.Email);
    }
}
```

### 3. Publish Events

Publish events from your command handlers:

```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _repository;
    private readonly IPublisher _publisher;
    
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var user = new User { Name = request.Name, Email = request.Email };
        await _repository.AddAsync(user);
        
        // Publish event - all handlers will be notified
        await _publisher.Publish(new UserCreatedEvent(user.Id, user.Name, user.Email), ct);
        
        return user.Id;
    }
}
```

## Key Interfaces

### INotification
Marker interface for all notifications:

```csharp
public interface INotification
{
}
```

### INotificationHandler<TNotification>
Handler interface for notifications:

```csharp
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}
```

### IPublisher
Interface for publishing notifications:

```csharp
public interface IPublisher
{
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) 
        where TNotification : INotification;
}
```

### IMediator
The mediator implements `IPublisher`:

```csharp
public interface IMediator : IPublisher
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
```

## Notification Patterns

### 1. Domain Events

Publish events when important domain actions occur:

```csharp
// Events
public record OrderPlacedEvent(Guid OrderId, Guid CustomerId, decimal Total) : INotification;
public record OrderShippedEvent(Guid OrderId, string TrackingNumber) : INotification;
public record OrderCancelledEvent(Guid OrderId, string Reason) : INotification;

// Handlers
public class OrderPlacedInventoryHandler : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent notification, CancellationToken ct)
    {
        // Update inventory
    }
}

public class OrderPlacedEmailHandler : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent notification, CancellationToken ct)
    {
        // Send confirmation email
    }
}

public class OrderPlacedAnalyticsHandler : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent notification, CancellationToken ct)
    {
        // Track analytics
    }
}
```

### 2. Integration Events

Communicate with external systems:

```csharp
public record CustomerRegisteredEvent(Guid CustomerId, string Email) : INotification;

public class CustomerRegisteredCrmHandler : INotificationHandler<CustomerRegisteredEvent>
{
    private readonly ICrmService _crmService;
    
    public async Task Handle(CustomerRegisteredEvent notification, CancellationToken ct)
    {
        await _crmService.SyncCustomerAsync(notification.CustomerId);
    }
}

public class CustomerRegisteredMarketingHandler : INotificationHandler<CustomerRegisteredEvent>
{
    private readonly IMarketingService _marketingService;
    
    public async Task Handle(CustomerRegisteredEvent notification, CancellationToken ct)
    {
        await _marketingService.AddToNewsletterAsync(notification.Email);
    }
}
```

### 3. Audit Events

Track changes for auditing:

```csharp
public record EntityChangedEvent(string EntityType, Guid EntityId, string Action, string UserId) : INotification;

public class AuditLogHandler : INotificationHandler<EntityChangedEvent>
{
    private readonly IAuditRepository _auditRepository;
    
    public async Task Handle(EntityChangedEvent notification, CancellationToken ct)
    {
        await _auditRepository.LogAsync(new AuditEntry
        {
            EntityType = notification.EntityType,
            EntityId = notification.EntityId,
            Action = notification.Action,
            UserId = notification.UserId,
            Timestamp = DateTime.UtcNow
        });
    }
}
```

## Handler Execution

### Parallel Execution

By default, all notification handlers run in parallel using `Task.WhenAll`:

```csharp
// All three handlers run simultaneously
await mediator.Publish(new UserCreatedEvent(userId, name, email));
```

### Benefits
- ✅ **Better performance** - handlers don't block each other
- ✅ **Fault isolation** - one handler's failure doesn't affect others
- ✅ **Scalability** - handlers can run on different threads

### Considerations
- ⚠️ **Order is not guaranteed** - handlers may complete in any order
- ⚠️ **Error handling** - if any handler fails, the entire publish fails

## Error Handling

### Option 1: Let Exceptions Bubble Up

```csharp
try
{
    await mediator.Publish(new UserCreatedEvent(userId, name, email));
}
catch (Exception ex)
{
    // One or more handlers failed
    _logger.LogError(ex, "Failed to publish UserCreatedEvent");
}
```

### Option 2: Resilient Handlers

Handle errors within each handler:

```csharp
public class ResilientEmailHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger _logger;
    
    public async Task Handle(UserCreatedEvent notification, CancellationToken ct)
    {
        try
        {
            await _emailService.SendWelcomeEmailAsync(notification.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", notification.Email);
            // Don't throw - allow other handlers to continue
        }
    }
}
```

### Option 3: Retry Logic

Use a retry library like Polly:

```csharp
public class RetryableHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IAsyncPolicy _retryPolicy;
    
    public RetryableHandler()
    {
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
    
    public async Task Handle(UserCreatedEvent notification, CancellationToken ct)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _externalService.NotifyAsync(notification);
        });
    }
}
```

## Testing Event Handlers

### Test Individual Handlers

```csharp
[Fact]
public async Task Handle_UserCreated_SendsEmail()
{
    // Arrange
    var emailService = new Mock<IEmailService>();
    var handler = new UserCreatedEmailHandler(emailService.Object);
    var notification = new UserCreatedEvent(Guid.NewGuid(), "John", "john@example.com");
    
    // Act
    await handler.Handle(notification, CancellationToken.None);
    
    // Assert
    emailService.Verify(x => x.SendWelcomeEmailAsync("john@example.com"), Times.Once);
}
```

### Test Event Publishing

```csharp
[Fact]
public async Task CreateUser_PublishesEvent()
{
    // Arrange
    var publisher = new Mock<IPublisher>();
    var handler = new CreateUserCommandHandler(repository, publisher.Object);
    var command = new CreateUserCommand("John", "john@example.com");
    
    // Act
    await handler.Handle(command, CancellationToken.None);
    
    // Assert
    publisher.Verify(x => x.Publish(
        It.Is<UserCreatedEvent>(e => e.Name == "John"), 
        It.IsAny<CancellationToken>()), 
        Times.Once);
}
```

## Best Practices

### 1. Use Records for Events

Events are immutable by nature:

```csharp
public record OrderPlacedEvent(Guid OrderId, DateTime PlacedAt) : INotification;
```

### 2. Keep Handlers Small and Focused

Each handler should do one thing:

```csharp
// ✅ Good - focused responsibility
public class OrderPlacedEmailHandler : INotificationHandler<OrderPlacedEvent>
{
    public Task Handle(OrderPlacedEvent notification, CancellationToken ct)
    {
        return _emailService.SendOrderConfirmationAsync(notification.OrderId);
    }
}

// ❌ Bad - too many responsibilities
public class OrderPlacedHandler : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent notification, CancellationToken ct)
    {
        await _emailService.SendEmailAsync(...);
        await _inventoryService.UpdateStockAsync(...);
        await _analyticsService.TrackAsync(...);
        await _crmService.SyncAsync(...);
    }
}
```

### 3. Name Events in Past Tense

Events describe something that has already happened:

```csharp
// ✅ Good
public record UserCreatedEvent(...) : INotification;
public record OrderShippedEvent(...) : INotification;
public record PaymentProcessedEvent(...) : INotification;

// ❌ Bad
public record CreateUserEvent(...) : INotification;
public record ShipOrderEvent(...) : INotification;
```

### 4. Include Relevant Data

Include all data handlers might need:

```csharp
// ✅ Good - includes all relevant data
public record UserCreatedEvent(
    Guid UserId, 
    string Name, 
    string Email,
    DateTime CreatedAt,
    string CreatedBy
) : INotification;

// ❌ Bad - handlers would need to query for data
public record UserCreatedEvent(Guid UserId) : INotification;
```

### 5. Don't Modify State in Event Handlers

Event handlers should be side-effect operations:

```csharp
// ✅ Good - sends notification, doesn't modify state
public class OrderShippedEmailHandler : INotificationHandler<OrderShippedEvent>
{
    public Task Handle(OrderShippedEvent notification, CancellationToken ct)
    {
        return _emailService.SendShippingNotificationAsync(notification);
    }
}

// ❌ Bad - modifies state that should be in command handler
public class OrderShippedHandler : INotificationHandler<OrderShippedEvent>
{
    public async Task Handle(OrderShippedEvent notification, CancellationToken ct)
    {
        var order = await _repository.GetAsync(notification.OrderId);
        order.Status = OrderStatus.Shipped; // This should be in the command handler!
        await _repository.UpdateAsync(order);
    }
}
```

## Advanced Patterns

### Event Sourcing Integration

```csharp
public record DomainEventOccurred(string AggregateId, string EventType, string EventData) : INotification;

public class EventStoreHandler : INotificationHandler<DomainEventOccurred>
{
    private readonly IEventStore _eventStore;
    
    public async Task Handle(DomainEventOccurred notification, CancellationToken ct)
    {
        await _eventStore.AppendAsync(
            notification.AggregateId,
            notification.EventType,
            notification.EventData);
    }
}
```

### Saga Pattern

```csharp
public record PaymentProcessedEvent(Guid OrderId, decimal Amount) : INotification;

public class OrderSagaHandler : INotificationHandler<PaymentProcessedEvent>
{
    private readonly IMediator _mediator;
    
    public async Task Handle(PaymentProcessedEvent notification, CancellationToken ct)
    {
        // Continue saga by sending next command
        await _mediator.Send(new ShipOrderCommand(notification.OrderId), ct);
    }
}
```

### Cross-Bounded Context Communication

```csharp
public record CustomerMovedEvent(Guid CustomerId, string NewAddress) : INotification;

public class ShippingContextHandler : INotificationHandler<CustomerMovedEvent>
{
    public async Task Handle(CustomerMovedEvent notification, CancellationToken ct)
    {
        // Update shipping context
        await _shippingRepository.UpdateCustomerAddressAsync(
            notification.CustomerId, 
            notification.NewAddress);
    }
}
```

## Performance Considerations

- Handlers run in parallel by default
- Keep handlers fast - offload heavy work to background jobs
- Consider using message queues for long-running operations
- Use cancellation tokens to allow early termination
- Monitor handler execution times

## Comparison with Request/Response

| Feature | Request/Response | Notifications/Events |
|---------|------------------|---------------------|
| Handlers | One handler | Multiple handlers |
| Execution | Sequential (with pipeline) | Parallel |
| Return value | Returns response | No return value |
| Purpose | Query data or execute command | Notify of state changes |
| Coupling | Direct coupling to handler | Loose coupling |
| Use case | CRUD operations | Side effects, integration |
