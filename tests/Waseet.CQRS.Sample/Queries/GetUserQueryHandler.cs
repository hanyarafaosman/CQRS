using Waseet.CQRS;
using Waseet.CQRS.Sample.Data;
using Waseet.CQRS.Sample.Models;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Handler for GetUserQuery.
/// </summary>
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly UserRepository _repository;

    public GetUserQueryHandler(UserRepository repository)
    {
        _repository = repository;
    }

    public Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        var user = _repository.GetById(request.UserId);
        
        if (user == null)
            throw new InvalidOperationException($"User with ID {request.UserId} not found");

        return Task.FromResult(user);
    }
}
