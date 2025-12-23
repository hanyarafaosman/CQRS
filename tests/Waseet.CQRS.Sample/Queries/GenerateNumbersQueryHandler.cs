using Waseet.CQRS;
using System.Runtime.CompilerServices;

namespace Waseet.CQRS.Sample.Queries;

/// <summary>
/// Handler for GenerateNumbersQuery - demonstrates streaming with async generation.
/// </summary>
public class GenerateNumbersQueryHandler : IStreamRequestHandler<GenerateNumbersQuery, int>
{
    public async IAsyncEnumerable<int> Handle(
        GenerateNumbersQuery request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 1; i <= request.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Simulate async work
            await Task.Delay(request.DelayMs, cancellationToken);
            
            yield return i;
        }
    }
}
