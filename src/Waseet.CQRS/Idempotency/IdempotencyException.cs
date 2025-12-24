namespace Waseet.CQRS.Idempotency;

/// <summary>
/// Exception thrown when idempotency validation fails.
/// </summary>
public class IdempotencyException : Exception
{
    /// <summary>
    /// Gets the request type name.
    /// </summary>
    public string RequestName { get; }

    /// <summary>
    /// Initializes a new instance of the IdempotencyException class.
    /// </summary>
    public IdempotencyException(string requestName, string message) 
        : base(message)
    {
        RequestName = requestName;
    }
}
