using Waseet.CQRS;

namespace Waseet.CQRS.Sample.Events;

/// <summary>
/// Event that is published when a user is updated.
/// </summary>
public record UserUpdatedEvent(Guid UserId, string NewName) : INotification;
