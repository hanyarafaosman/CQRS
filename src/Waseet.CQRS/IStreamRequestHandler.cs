using System.Runtime.CompilerServices;

namespace Waseet.CQRS;

/// <summary>
/// Defines a handler for a stream request.
/// </summary>
/// <typeparam name="TRequest">The type of stream request being handled.</typeparam>
/// <typeparam name="TResponse">The type of items in the response stream.</typeparam>
public interface IStreamRequestHandler<in TRequest, out TResponse> where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Handles the stream request and returns an async stream of responses.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of responses.</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default);
}
