namespace Waseet.CQRS.Idempotency;

/// <summary>
/// Interface for requests that provide their own idempotency key.
/// </summary>
public interface IIdempotentRequest
{
    /// <summary>
    /// Gets the idempotency key for this request.
    /// </summary>
    string IdempotencyKey { get; }
}
