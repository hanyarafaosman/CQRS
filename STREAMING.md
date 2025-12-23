# Stream Support Guide

## Overview
The CQRS.Mediator library supports streaming data with `IAsyncEnumerable<T>`, allowing you to process large datasets efficiently by returning data incrementally rather than loading everything into memory.

## Quick Start

### 1. Define a Stream Request

Stream requests implement `IStreamRequest<TResponse>`:

```csharp
using CQRS.Mediator;

public record StreamAllUsersQuery() : IStreamRequest<User>;
public record StreamLargeDatasetQuery(int BatchSize) : IStreamRequest<DataItem>;
```

### 2. Create a Stream Handler

Implement `IStreamRequestHandler<TRequest, TResponse>`:

```csharp
using CQRS.Mediator;
using System.Runtime.CompilerServices;

public class StreamAllUsersQueryHandler : IStreamRequestHandler<StreamAllUsersQuery, User>
{
    private readonly IUserRepository _repository;

    public async IAsyncEnumerable<User> Handle(
        StreamAllUsersQuery request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Stream users from database
        await foreach (var user in _repository.StreamUsersAsync(cancellationToken))
        {
            yield return user;
        }
    }
}
```

### 3. Consume the Stream

Use `await foreach` to consume items as they arrive:

```csharp
var mediator = serviceProvider.GetRequiredService<IMediator>();

await foreach (var user in mediator.CreateStream(new StreamAllUsersQuery()))
{
    Console.WriteLine($"Processing user: {user.Name}");
    // Process each user as it arrives
}
```

## Key Interfaces

### IStreamRequest<TResponse>
Marker interface for stream requests:

```csharp
public interface IStreamRequest<out TResponse>
{
}
```

### IStreamRequestHandler<TRequest, TResponse>
Handler interface for stream requests:

```csharp
public interface IStreamRequestHandler<in TRequest, out TResponse> 
    where TRequest : IStreamRequest<TResponse>
{
    IAsyncEnumerable<TResponse> Handle(
        TRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
}
```

### IMediator.CreateStream<TResponse>
Method to create a stream from a stream request:

```csharp
public interface IMediator
{
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request, 
        CancellationToken cancellationToken = default);
}
```

## Use Cases

### 1. Large Dataset Processing

Stream large datasets without loading everything into memory:

```csharp
// Request
public record StreamProductsQuery(int MinStock) : IStreamRequest<Product>;

// Handler
public class StreamProductsQueryHandler : IStreamRequestHandler<StreamProductsQuery, Product>
{
    private readonly IProductRepository _repository;

    public async IAsyncEnumerable<Product> Handle(
        StreamProductsQuery request, 
        [EnumeratorCancellation] CancellationToken ct)
    {
        // Database streaming (avoids loading millions of records)
        await foreach (var product in _repository.StreamProductsAsync(request.MinStock, ct))
        {
            yield return product;
        }
    }
}

// Usage
await foreach (var product in mediator.CreateStream(new StreamProductsQuery(10)))
{
    // Process one product at a time
    await ProcessProductAsync(product);
}
```

### 2. File Processing

Process large files line by line:

```csharp
public record StreamFileL inesQuery(string FilePath) : IStreamRequest<string>;

public class StreamFileLinesQueryHandler : IStreamRequestHandler<StreamFileLinesQuery, string>
{
    public async IAsyncEnumerable<string> Handle(
        StreamFileLinesQuery request, 
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var reader = new StreamReader(request.FilePath);
        
        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync();
            if (line != null)
                yield return line;
        }
    }
}
```

### 3. Real-Time Data Feeds

Stream live data as it becomes available:

```csharp
public record StreamStockPricesQuery(string Symbol) : IStreamRequest<StockPrice>;

public class StreamStockPricesQueryHandler : IStreamRequestHandler<StreamStockPricesQuery, StockPrice>
{
    private readonly IStockService _stockService;

    public async IAsyncEnumerable<StockPrice> Handle(
        StreamStockPricesQuery request, 
        [EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var price = await _stockService.GetCurrentPriceAsync(request.Symbol);
            yield return price;
            
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }
    }
}
```

### 4. Paginated Data

Stream paginated data seamlessly:

```csharp
public record StreamPaginatedUsersQuery(int PageSize = 100) : IStreamRequest<User>;

public class StreamPaginatedUsersQueryHandler : IStreamRequestHandler<StreamPaginatedUsersQuery, User>
{
    private readonly IUserRepository _repository;

    public async IAsyncEnumerable<User> Handle(
        StreamPaginatedUsersQuery request, 
        [EnumeratorCancellation] CancellationToken ct)
    {
        int page = 0;
        bool hasMore = true;

        while (hasMore)
        {
            var users = await _repository.GetPageAsync(page, request.PageSize, ct);
            
            if (users.Count == 0)
            {
                hasMore = false;
                break;
            }

            foreach (var user in users)
            {
                yield return user;
            }

            page++;
        }
    }
}
```

## Benefits

### Memory Efficiency
```csharp
// ❌ Bad - Loads all data into memory
public Task<List<User>> Handle(GetAllUsersQuery request)
{
    return _repository.GetAllAsync(); // Could be millions of records!
}

// ✅ Good - Streams data one item at a time
public async IAsyncEnumerable<User> Handle(StreamAllUsersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    await foreach (var user in _repository.StreamAsync(ct))
    {
        yield return user; // Only one user in memory at a time
    }
}
```

### Early Processing
Start processing data as soon as the first item arrives:

```csharp
await foreach (var item in mediator.CreateStream(new StreamDataQuery()))
{
    // Start processing immediately - don't wait for all data
    await ProcessItemAsync(item);
}
```

### Cancellation Support
Cancel long-running streams:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

try
{
    await foreach (var item in mediator.CreateStream(new StreamDataQuery(), cts.Token))
    {
        await ProcessItemAsync(item);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream cancelled after timeout");
}
```

## Pipeline Behaviors for Streams

You can create pipeline behaviors for stream requests using `IStreamPipelineBehavior<TRequest, TResponse>`:

```csharp
public class StreamLoggingBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly ILogger _logger;

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request, 
        StreamHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken ct)
    {
        _logger.LogInformation("Starting stream for {RequestType}", typeof(TRequest).Name);
        
        int count = 0;
        
        await foreach (var item in next().WithCancellation(ct))
        {
            count++;
            yield return item;
        }
        
        _logger.LogInformation("Stream completed. Items streamed: {Count}", count);
    }
}
```

Register stream behaviors:

```csharp
services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>));
```

## Best Practices

### 1. Use EnumeratorCancellation Attribute

Always use `[EnumeratorCancellation]` on the cancellation token parameter:

```csharp
public async IAsyncEnumerable<T> Handle(
    TRequest request, 
    [EnumeratorCancellation] CancellationToken ct) // ✅ Correct
{
    // Implementation
}
```

### 2. Check Cancellation in Loops

For long-running or CPU-intensive operations:

```csharp
public async IAsyncEnumerable<int> Handle(
    GenerateNumbersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    for (int i = 0; i < request.Count; i++)
    {
        ct.ThrowIfCancellationRequested(); // ✅ Check cancellation
        
        yield return i;
    }
}
```

### 3. Avoid Buffering

Don't collect all items before yielding:

```csharp
// ❌ Bad - Defeats the purpose of streaming
public async IAsyncEnumerable<User> Handle(StreamUsersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    var allUsers = await _repository.GetAllAsync(); // Loads everything!
    
    foreach (var user in allUsers)
    {
        yield return user;
    }
}

// ✅ Good - True streaming
public async IAsyncEnumerable<User> Handle(StreamUsersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    await foreach (var user in _repository.StreamAsync(ct))
    {
        yield return user; // Streams one at a time
    }
}
```

### 4. Handle Errors Gracefully

```csharp
public async IAsyncEnumerable<User> Handle(
    StreamUsersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    await foreach (var user in _repository.StreamAsync(ct))
    {
        // Validate or transform
        if (user.IsActive)
        {
            yield return user;
        }
    }
}
```

### 5. Use ConfigureAwait When Appropriate

```csharp
public async IAsyncEnumerable<User> Handle(
    StreamUsersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    await foreach (var user in _repository.StreamAsync(ct).ConfigureAwait(false))
    {
        yield return user;
    }
}
```

## Testing Stream Handlers

### Test Individual Handlers

```csharp
[Fact]
public async Task Handle_StreamsAllUsers()
{
    // Arrange
    var repository = new Mock<IUserRepository>();
    var users = new[] { new User { Name = "John" }, new User { Name = "Jane" } };
    repository.Setup(x => x.StreamAsync(It.IsAny<CancellationToken>()))
              .Returns(users.ToAsyncEnumerable());
    
    var handler = new StreamAllUsersQueryHandler(repository.Object);
    var query = new StreamAllUsersQuery();
    
    // Act
    var result = new List<User>();
    await foreach (var user in handler.Handle(query, CancellationToken.None))
    {
        result.Add(user);
    }
    
    // Assert
    Assert.Equal(2, result.Count);
    Assert.Contains(result, u => u.Name == "John");
}
```

### Test with Mediator

```csharp
[Fact]
public async Task CreateStream_ReturnsExpectedItems()
{
    // Arrange
    var mediator = serviceProvider.GetRequiredService<IMediator>();
    var query = new GenerateNumbersQuery(5);
    
    // Act
    var numbers = new List<int>();
    await foreach (var number in mediator.CreateStream(query))
    {
        numbers.Add(number);
    }
    
    // Assert
    Assert.Equal(new[] { 1, 2, 3, 4, 5 }, numbers);
}
```

## Performance Considerations

### Streaming vs Batch Loading

| Scenario | Stream | Batch |
|----------|--------|-------|
| 1M records | ✅ Low memory | ❌ High memory |
| First item latency | ✅ Fast | ❌ Slow |
| Total processing time | Similar | Similar |
| Backpressure support | ✅ Yes | ❌ No |
| Cancellation | ✅ Immediate | ❌ After batch |

### When to Use Streaming

✅ **Use Streaming When:**
- Processing large datasets (>1000 items)
- Data source supports streaming (database cursors, file I/O)
- Memory is constrained
- You need to start processing early
- Long-running operations that may be cancelled

❌ **Don't Use Streaming When:**
- Small result sets (<100 items)
- Need all data for calculations (aggregations, sorting)
- Data source doesn't support streaming
- Results need to be displayed together (UI grids)

## Advanced Patterns

### Batch Processing Within Streams

```csharp
public async IAsyncEnumerable<Batch<User>> Handle(
    StreamUserBatchesQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    var batch = new List<User>();
    
    await foreach (var user in _repository.StreamAsync(ct))
    {
        batch.Add(user);
        
        if (batch.Count >= request.BatchSize)
        {
            yield return new Batch<User>(batch);
            batch = new List<User>();
        }
    }
    
    if (batch.Count > 0)
        yield return new Batch<User>(batch);
}
```

### Filtering and Transformation

```csharp
public async IAsyncEnumerable<UserDto> Handle(
    StreamActiveUsersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    await foreach (var user in _repository.StreamAsync(ct))
    {
        if (!user.IsActive)
            continue; // Skip inactive users
        
        yield return new UserDto
        {
            Id = user.Id,
            Name = user.Name
        };
    }
}
```

### Merging Multiple Streams

```csharp
public async IAsyncEnumerable<Event> Handle(
    StreamAllEventsQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    var stream1 = _repository1.StreamEventsAsync(ct);
    var stream2 = _repository2.StreamEventsAsync(ct);
    
    await foreach (var item in MergeStreamsAsync(stream1, stream2, ct))
    {
        yield return item;
    }
}
```

## Comparison with Regular Requests

| Feature | Regular Request | Stream Request |
|---------|----------------|----------------|
| Return Type | `Task<TResponse>` | `IAsyncEnumerable<TResponse>` |
| Memory Usage | All in memory | One at a time |
| First Item | After completion | Immediately |
| Cancellation | After completion | During iteration |
| Use Case | Small results | Large results |
| Interface | `IRequest<T>` | `IStreamRequest<T>` |
| Handler | `IRequestHandler<,>` | `IStreamRequestHandler<,>` |
| Mediator Method | `Send<T>()` | `CreateStream<T>()` |

## Database Integration

### Entity Framework Core

```csharp
public async IAsyncEnumerable<User> Handle(
    StreamAllUsersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    // AsAsyncEnumerable() enables true streaming from DB
    await foreach (var user in _context.Users.AsAsyncEnumerable().WithCancellation(ct))
    {
        yield return user;
    }
}
```

### Dapper

```csharp
public async IAsyncEnumerable<User> Handle(
    StreamAllUsersQuery request, 
    [EnumeratorCancellation] CancellationToken ct)
{
    using var connection = new SqlConnection(_connectionString);
    using var reader = await connection.ExecuteReaderAsync("SELECT * FROM Users");
    
    while (await reader.ReadAsync(ct))
    {
        yield return new User
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1)
        };
    }
}
```
