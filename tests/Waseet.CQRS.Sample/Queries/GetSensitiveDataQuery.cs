using Waseet.CQRS;
using Waseet.CQRS.Authorization;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Query that requires specific policy - uses request name as policy if not specified.
/// </summary>
[Authorize] // Will check for "GetSensitiveDataQuery" policy
public record GetSensitiveDataQuery : IRequest<string>;
