using Waseet.CQRS;

namespace Waseet.CQRS.Sample.Queries;

public class GetSensitiveDataQueryHandler : IRequestHandler<GetSensitiveDataQuery, string>
{
    public Task<string> Handle(GetSensitiveDataQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("🔒 This is sensitive data that requires authorization!");
    }
}
