using Waseet.CQRS;
using Waseet.CQRS.Sample.Data;
using Waseet.CQRS.Sample.Events;
using Waseet.CQRS.Sample.Models;

namespace Waseet.CQRS.Sample.Commands;

/// <summary>
/// Handler for CreateUserCommand.
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly UserRepository _repository;
    private readonly IPublisher _publisher;

    public CreateUserCommandHandler(UserRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email
        };

        _repository.Add(user);

        // Publish event
        await _publisher.Publish(new UserCreatedEvent(user.Id, user.Name, user.Email), cancellationToken);

        return user.Id;
    }
}
