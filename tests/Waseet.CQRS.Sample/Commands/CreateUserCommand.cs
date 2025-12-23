using Waseet.CQRS;
using Waseet.CQRS.Auditing;

namespace Waseet.CQRS.Sample.Commands;

/// <summary>
/// Command to create a new user.
/// </summary>
[Audit(IncludeRequest = true, IncludeResponse = true, Category = "UserManagement", Tags = new[] { "User", "Create" })]
public record CreateUserCommand(string Name, string Email) : IRequest<Guid>;
