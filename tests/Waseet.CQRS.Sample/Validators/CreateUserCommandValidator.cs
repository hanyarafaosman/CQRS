using Waseet.CQRS.Sample.Commands;
using Waseet.CQRS.Validation;

namespace Waseet.CQRS.Sample.Validators;

/// <summary>
/// Validator for CreateUserCommand.
/// </summary>
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public ValidationResult Validate(CreateUserCommand request)
    {
        return ValidateAsync(request).GetAwaiter().GetResult();
    }

    public Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationError(nameof(request.Name), "Name is required"));
        }
        else if (request.Name.Length < 2)
        {
            errors.Add(new ValidationError(nameof(request.Name), "Name must be at least 2 characters"));
        }
        else if (request.Name.Length > 100)
        {
            errors.Add(new ValidationError(nameof(request.Name), "Name must not exceed 100 characters"));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add(new ValidationError(nameof(request.Email), "Email is required"));
        }
        else if (!request.Email.Contains("@"))
        {
            errors.Add(new ValidationError(nameof(request.Email), "Email must be a valid email address"));
        }

        return Task.FromResult(errors.Count > 0 
            ? ValidationResult.Failure(errors.ToArray()) 
            : ValidationResult.Success());
    }
}
