using Waseet.CQRS;
using Waseet.CQRS.Auditing;
using Waseet.CQRS.Authorization;
using Waseet.CQRS.Monitoring;
using Waseet.CQRS.Sample.Data;

namespace Waseet.CQRS.Sample.Commands;

/// <summary>
/// Admin-only command that is fully monitored and audited.
/// </summary>
[Authorize(Roles = "Admin")]
[Monitor(SlowThresholdMs = 500, IncludeRequestData = true)]
[Audit(IncludeRequest = true, IncludeResponse = true, Category = "Security", Tags = new[] { "Admin", "UserDeletion" })]
public record DeleteUserAuditedCommand(Guid UserId) : IRequest<bool>;

public class DeleteUserAuditedCommandHandler : IRequestHandler<DeleteUserAuditedCommand, bool>
{
    private readonly UserRepository _repository;

    public DeleteUserAuditedCommandHandler(UserRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> Handle(DeleteUserAuditedCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  🗑️ Deleting user: {request.UserId}");
        var user = _repository.GetById(request.UserId);
        
        if (user == null)
        {
            return Task.FromResult(false);
        }

        _repository.Delete(request.UserId);
        return Task.FromResult(true);
    }
}
