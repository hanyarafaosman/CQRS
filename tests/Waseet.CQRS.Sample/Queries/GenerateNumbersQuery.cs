using Waseet.CQRS;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Stream query to generate a sequence of numbers.
/// </summary>
public record GenerateNumbersQuery(int Count, int DelayMs = 50) : IStreamRequest<int>;
