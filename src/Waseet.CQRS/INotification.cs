namespace Waseet.CQRS;

/// <summary>
/// Marker interface for notifications that don't return a response.
/// Notifications are published to multiple handlers (pub/sub pattern).
/// </summary>
public interface INotification
{
}
