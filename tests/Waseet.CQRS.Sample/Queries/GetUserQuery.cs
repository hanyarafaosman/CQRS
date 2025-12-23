using Waseet.CQRS;
using Waseet.CQRS.Sample.Models;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Query to get a user by ID.
/// </summary>
public record GetUserQuery(Guid UserId) : IRequest<User>;
