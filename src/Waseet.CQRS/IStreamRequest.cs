namespace Waseet.CQRS;

/// <summary>
/// Marker interface for stream requests that return a stream of responses.
/// </summary>
/// <typeparam name="TResponse">The type of items in the stream.</typeparam>
public interface IStreamRequest<out TResponse>
{
}
