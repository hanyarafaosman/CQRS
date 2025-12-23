namespace Waseet.CQRS.Validation;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    public ValidationException(IEnumerable<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    public ValidationException(ValidationResult validationResult)
        : this(validationResult.Errors)
    {
    }

    /// <summary>
    /// Gets a formatted error message with all validation errors.
    /// </summary>
    public override string Message
    {
        get
        {
            if (Errors.Count == 0)
                return base.Message;

            var errors = string.Join(Environment.NewLine, 
                Errors.Select(e => $"- {e.PropertyName}: {e.ErrorMessage}"));

            return $"{base.Message}{Environment.NewLine}{errors}";
        }
    }
}
