using Microsoft.Extensions.DependencyInjection;

namespace Waseet.CQRS;

/// <summary>
/// Default implementation of IMediator.
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the Mediator class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve handlers.</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Sends a request to its handler and returns the response.
    /// </summary>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        // Get pipeline behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = _serviceProvider.GetServices(behaviorType).ToList();

        // Build the handler pipeline
        RequestHandlerDelegate<TResponse> handler = async () =>
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            var requestHandler = _serviceProvider.GetService(handlerType);

            if (requestHandler == null)
                throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");

            var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));

            if (handleMethod == null)
                throw new InvalidOperationException($"Handle method not found on handler for request type {requestType.Name}");

            var result = handleMethod.Invoke(requestHandler, new object[] { request, cancellationToken });

            if (result is Task<TResponse> task)
                return await task;

            throw new InvalidOperationException($"Handler for {requestType.Name} did not return a Task<{responseType.Name}>");
        };

        // Execute pipeline behaviors in reverse order
        for (int i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var currentHandler = handler;
            handler = () =>
            {
                var handleMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle));
                if (handleMethod == null)
                    throw new InvalidOperationException("Handle method not found on pipeline behavior");

                var result = handleMethod.Invoke(behavior, new object[] { request, currentHandler, cancellationToken });
                if (result is Task<TResponse> task)
                    return task;

                throw new InvalidOperationException("Pipeline behavior did not return a Task<TResponse>");
            };
        }

        return await handler();
    }

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));

        return PublishCore(notification, cancellationToken);
    }

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));

        return PublishCore(notification, cancellationToken);
    }

    private async Task PublishCore(INotification notification, CancellationToken cancellationToken)
    {
        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        var handlers = _serviceProvider.GetServices(handlerType);

        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<INotification>.Handle));

            if (handleMethod == null)
                continue;

            var result = handleMethod.Invoke(handler, new object[] { notification, cancellationToken });

            if (result is Task task)
                tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Creates a stream from a stream request handler.
    /// </summary>
    public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        // Get stream pipeline behaviors
        var behaviorType = typeof(IStreamPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = _serviceProvider.GetServices(behaviorType).ToList();

        // Build the handler pipeline
        StreamHandlerDelegate<TResponse> handler = () =>
        {
            var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, responseType);
            var requestHandler = _serviceProvider.GetService(handlerType);

            if (requestHandler == null)
                throw new InvalidOperationException($"No stream handler registered for request type {requestType.Name}");

            var handleMethod = handlerType.GetMethod(nameof(IStreamRequestHandler<IStreamRequest<TResponse>, TResponse>.Handle));

            if (handleMethod == null)
                throw new InvalidOperationException($"Handle method not found on stream handler for request type {requestType.Name}");

            var result = handleMethod.Invoke(requestHandler, new object[] { request, cancellationToken });

            if (result is IAsyncEnumerable<TResponse> stream)
                return stream;

            throw new InvalidOperationException($"Stream handler for {requestType.Name} did not return an IAsyncEnumerable<{responseType.Name}>");
        };

        // Execute pipeline behaviors in reverse order
        for (int i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var currentHandler = handler;
            handler = () =>
            {
                var handleMethod = behaviorType.GetMethod(nameof(IStreamPipelineBehavior<IStreamRequest<TResponse>, TResponse>.Handle));
                if (handleMethod == null)
                    throw new InvalidOperationException("Handle method not found on stream pipeline behavior");

                var result = handleMethod.Invoke(behavior, new object[] { request, currentHandler, cancellationToken });
                if (result is IAsyncEnumerable<TResponse> stream)
                    return stream;

                throw new InvalidOperationException("Stream pipeline behavior did not return an IAsyncEnumerable<TResponse>");
            };
        }

        // Execute the pipeline and yield results
        await foreach (var item in handler().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
}
