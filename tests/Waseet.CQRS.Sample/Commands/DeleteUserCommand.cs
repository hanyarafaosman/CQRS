using Waseet.CQRS;
using Waseet.CQRS.Authorization;

namespace Waseet.CQRS.Sample.Commands;

/// <summary>
/// Command to delete a user - requires "Admin" role.
/// </summary>
[Authorize(Roles = "Admin")]
public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;
