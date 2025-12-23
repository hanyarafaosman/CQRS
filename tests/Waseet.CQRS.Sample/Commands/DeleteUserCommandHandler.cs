using Waseet.CQRS;
using Waseet.CQRS.Sample.Data;

namespace Waseet.CQRS.Sample.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly UserRepository _repository;

    public DeleteUserCommandHandler(UserRepository repository)
    {
        _repository = repository;
    }

    public Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = _repository.GetById(request.UserId);
        if (user != null)
        {
            _repository.Delete(request.UserId);
            Console.WriteLine($"   User deleted: {user.Name}");
        }
        
        return Task.FromResult(Unit.Value);
    }
}
