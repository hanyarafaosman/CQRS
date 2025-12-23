namespace Waseet.CQRS;

/// <summary>
/// Defines a mediator to send requests and publish notifications.
/// </summary>
public interface IMediator : IPublisher
{
    /// <summary>
    /// Sends a request to a single handler and returns a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a stream from a stream request handler.
    /// </summary>
    /// <typeparam name="TResponse">The type of items in the stream.</typeparam>
    /// <param name="request">The stream request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of responses.</returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);
}
