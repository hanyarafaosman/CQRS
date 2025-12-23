using Microsoft.Extensions.DependencyInjection;

namespace Waseet.CQRS.Validation;

/// <summary>
/// Pipeline behavior that validates requests before they are handled.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the ValidationBehavior class.
    /// </summary>
    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Validates the request before calling the next behavior in the pipeline.
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Get all validators for this request type
        var validators = _serviceProvider.GetServices<IValidator<TRequest>>().ToList();

        if (validators.Count == 0)
        {
            // No validators registered, continue to next behavior
            return await next();
        }

        // Run all validators
        var validationTasks = validators.Select(v => v.ValidateAsync(request, cancellationToken));
        var validationResults = await Task.WhenAll(validationTasks);

        // Collect all errors
        var errors = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        // All validations passed, continue to next behavior
        return await next();
    }
}
