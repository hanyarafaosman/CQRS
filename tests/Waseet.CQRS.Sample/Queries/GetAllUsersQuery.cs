using Waseet.CQRS;
using Waseet.CQRS.Caching;
using Waseet.CQRS.Monitoring;
using Waseet.CQRS.Sample.Models;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Query to get all users.
/// Cached for 1 minute and monitored for performance.
/// </summary>
[Cache(Duration = 60)] // Cache for 1 minute
[Monitor(SlowThresholdMs = 100)]
public record GetAllUsersQuery() : IRequest<List<User>>;
