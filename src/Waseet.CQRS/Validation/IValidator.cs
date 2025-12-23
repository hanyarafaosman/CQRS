namespace Waseet.CQRS.Validation;

/// <summary>
/// Defines a validator for a request.
/// </summary>
/// <typeparam name="TRequest">The type of request to validate.</typeparam>
public interface IValidator<in TRequest>
{
    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(TRequest request);

    /// <summary>
    /// Validates the request asynchronously.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}
