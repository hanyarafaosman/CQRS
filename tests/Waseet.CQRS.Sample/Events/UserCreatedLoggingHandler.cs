using Waseet.CQRS;

namespace Waseet.CQRS.Sample.Events;

/// <summary>
/// Handler that logs user created events to console.
/// </summary>
public class UserCreatedLoggingHandler : INotificationHandler<UserCreatedEvent>
{
    public Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[LOG] User created: {notification.Name} ({notification.Email}) with ID: {notification.UserId}");
        return Task.CompletedTask;
    }
}
