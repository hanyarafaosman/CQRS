using Microsoft.Extensions.DependencyInjection;

namespace Waseet.CQRS.Idempotency;

/// <summary>
/// Pipeline behavior that handles idempotency for commands.
/// </summary>
public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public IdempotencyBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var idempotentAttr = requestType.GetCustomAttributes(typeof(IdempotentAttribute), true)
            .Cast<IdempotentAttribute>()
            .FirstOrDefault();

        if (idempotentAttr == null)
        {
            return await next();
        }

        var store = _serviceProvider.GetService<IIdempotencyStore>();
        if (store == null)
        {
            return await next();
        }

        // Extract idempotency key
        var idempotencyKey = ExtractIdempotencyKey(request, requestType, idempotentAttr);
        
        if (string.IsNullOrEmpty(idempotencyKey))
        {
            throw new IdempotencyException(
                requestType.Name,
                $"Idempotency key is required for request {requestType.Name}. " +
                $"Implement IIdempotentRequest or provide a property named 'IdempotencyKey'.");
        }

        // Check if already processed
        var existingResult = await store.GetAsync<TResponse>(idempotencyKey, cancellationToken);
        if (existingResult != null)
        {
            Console.WriteLine($"🔄 Idempotent request detected: {requestType.Name} with key '{idempotencyKey}' (processed at {existingResult.CreatedAt:yyyy-MM-dd HH:mm:ss})");
            return existingResult.Response;
        }

        // Execute handler
        var response = await next();

        // Store result
        await store.SetAsync(
            idempotencyKey,
            response,
            TimeSpan.FromSeconds(idempotentAttr.Duration),
            cancellationToken);

        return response;
    }

    private string? ExtractIdempotencyKey(TRequest request, Type requestType, IdempotentAttribute attribute)
    {
        // Check if implements IIdempotentRequest
        if (request is IIdempotentRequest idempotentRequest)
        {
            return idempotentRequest.IdempotencyKey;
        }

        // Check for property specified in attribute
        var propertyName = attribute.KeyProperty ?? "IdempotencyKey";
        var property = requestType.GetProperty(propertyName);
        
        if (property == null)
        {
            return null;
        }

        var value = property.GetValue(request);
        return value?.ToString();
    }
}
