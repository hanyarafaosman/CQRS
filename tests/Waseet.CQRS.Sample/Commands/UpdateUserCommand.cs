using Waseet.CQRS;
using Waseet.CQRS.Caching;
using Waseet.CQRS.Auditing;

namespace Waseet.CQRS.Sample.Commands;

/// <summary>
/// Command to update a user. This command doesn't return a value.
/// Invalidates cache and is audited.
/// </summary>
[InvalidateCache("user-{UserId}", UsePattern = false)]
[InvalidateCache("GetAllUsersQuery", UsePattern = false)] // Clear all users cache
[Audit(IncludeRequest = true, Category = "UserManagement", Tags = new[] { "User", "Update" })]
public record UpdateUserCommand(Guid UserId, string NewName) : IRequest;
