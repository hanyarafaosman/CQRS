using Waseet.CQRS;
using Waseet.CQRS.Sample.Models;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Stream query to get users one at a time.
/// </summary>
public record StreamAllUsersQuery() : IStreamRequest<User>;
