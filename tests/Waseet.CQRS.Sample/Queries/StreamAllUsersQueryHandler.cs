using Waseet.CQRS;
using Waseet.CQRS.Sample.Data;
using Waseet.CQRS.Sample.Models;
using System.Runtime.CompilerServices;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Handler for StreamAllUsersQuery - returns users as a stream.
/// </summary>
public class StreamAllUsersQueryHandler : IStreamRequestHandler<StreamAllUsersQuery, User>
{
    private readonly UserRepository _repository;

    public StreamAllUsersQueryHandler(UserRepository repository)
    {
        _repository = repository;
    }

    public async IAsyncEnumerable<User> Handle(
        StreamAllUsersQuery request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var users = _repository.GetAll();

        foreach (var user in users)
        {
            // Simulate async processing/loading
            await Task.Delay(100, cancellationToken);
            
            yield return user;
        }
    }
}
