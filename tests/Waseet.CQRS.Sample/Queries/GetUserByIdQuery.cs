using Waseet.CQRS;
using Waseet.CQRS.Caching;
using Waseet.CQRS.Sample.Data;
using Waseet.CQRS.Sample.Models;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Cached query that gets user details.
/// Cache key includes UserId to cache each user separately.
/// </summary>
[Cache(Key = "user-{UserId}", Duration = 300)] // Cache for 5 minutes
public record GetUserByIdQuery(Guid UserId) : IRequest<User?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User?>
{
    private readonly UserRepository _repository;

    public GetUserByIdQueryHandler(UserRepository repository)
    {
        _repository = repository;
    }

    public Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  🔍 Fetching user from database: {request.UserId}");
        return Task.FromResult(_repository.GetById(request.UserId));
    }
}
