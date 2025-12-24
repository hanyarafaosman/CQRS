using Waseet.CQRS;
using Waseet.CQRS.Idempotency;
using Waseet.CQRS.Auditing;

namespace Waseet.CQRS.Sample.Commands;

/// <summary>
/// Command to create a payment with idempotency protection.
/// </summary>
[Idempotent(Duration = 3600)] // Prevent duplicates for 1 hour
[Audit(IncludeRequest = true, IncludeResponse = true, Category = "Payment", Tags = new[] { "Payment", "Create" })]
public record CreatePaymentCommand(
    string IdempotencyKey,
    decimal Amount,
    string Description) : IRequest<Guid>, IIdempotentRequest;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Guid>
{
    public Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  💳 Processing payment: {request.Amount:C} - {request.Description}");
        
        // Simulate payment processing
        var paymentId = Guid.NewGuid();
        
        Console.WriteLine($"  ✅ Payment processed with ID: {paymentId}");
        
        return Task.FromResult(paymentId);
    }
}
