using System.Runtime.CompilerServices;

namespace Waseet.CQRS;

/// <summary>
/// Pipeline behavior to surround stream request handler execution.
/// </summary>
/// <typeparam name="TRequest">The type of stream request.</typeparam>
/// <typeparam name="TResponse">The type of items in the response stream.</typeparam>
public interface IStreamPipelineBehavior<in TRequest, TResponse> where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Pipeline handler for stream requests. Perform pre-processing, call the next delegate, and perform post-processing.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of responses.</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken);
}

/// <summary>
/// Represents an async continuation for the next stream handler to execute in the pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of items in the response stream.</typeparam>
public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TResponse>();
