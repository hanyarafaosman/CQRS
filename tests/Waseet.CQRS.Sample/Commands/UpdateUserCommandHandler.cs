using Waseet.CQRS;
using Waseet.CQRS.Sample.Data;
using Waseet.CQRS.Sample.Events;

namespace Waseet.CQRS.Sample.Commands;

/// <summary>
/// Handler for UpdateUserCommand.
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly UserRepository _repository;
    private readonly IPublisher _publisher;

    public UpdateUserCommandHandler(UserRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = _repository.GetById(request.UserId);
        
        if (user != null)
        {
            user.Name = request.NewName;
            _repository.Update(user);

            // Publish event
            await _publisher.Publish(new UserUpdatedEvent(user.Id, user.Name), cancellationToken);
        }

        return Unit.Value;
    }
}
