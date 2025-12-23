using Waseet.CQRS;

namespace Waseet.CQRS.Sample.Events;

/// <summary>
/// Handler that sends welcome email when user is created.
/// </summary>
public class UserCreatedEmailHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        // Simulate sending email
        await Task.Delay(100, cancellationToken);
        Console.WriteLine($"[EMAIL] Welcome email sent to {notification.Email}");
    }
}
