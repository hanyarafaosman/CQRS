using Waseet.CQRS;
using Waseet.CQRS.Sample.Data;
using Waseet.CQRS.Sample.Models;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Handler for GetAllUsersQuery.
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<User>>
{
    private readonly UserRepository _repository;

    public GetAllUsersQueryHandler(UserRepository repository)
    {
        _repository = repository;
    }

    public Task<List<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken = default)
    {
        var users = _repository.GetAll();
        return Task.FromResult(users);
    }
}
