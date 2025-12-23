using Waseet.CQRS;

namespace Waseet.CQRS.Sample.Events;

/// <summary>
/// Event that is published when a user is created.
/// </summary>
public record UserCreatedEvent(Guid UserId, string Name, string Email) : INotification;
