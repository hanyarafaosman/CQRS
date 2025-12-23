using Waseet.CQRS;

namespace Waseet.CQRS.Sample.Events;

/// <summary>
/// Handler that logs user updated events to console.
/// </summary>
public class UserUpdatedLoggingHandler : INotificationHandler<UserUpdatedEvent>
{
    public Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[LOG] User {notification.UserId} updated to name: {notification.NewName}");
        return Task.CompletedTask;
    }
}
