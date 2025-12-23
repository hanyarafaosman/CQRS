using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Waseet.CQRS.Caching;

/// <summary>
/// Pipeline behavior that handles response caching and cache invalidation.
/// </summary>
public class CacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public CacheBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        
        // Check for cache invalidation first (for commands)
        await HandleCacheInvalidation(request, requestType, cancellationToken);
        
        // Check for caching (for queries)
        var cacheAttr = requestType.GetCustomAttributes(typeof(CacheAttribute), true)
            .Cast<CacheAttribute>()
            .FirstOrDefault();

        if (cacheAttr == null)
        {
            return await next();
        }

        var cacheProvider = _serviceProvider.GetService<ICacheProvider>();
        if (cacheProvider == null)
        {
            return await next();
        }

        // Generate cache key
        var cacheKey = GenerateCacheKey(request, requestType, cacheAttr.Key);

        // Try to get from cache
        var cachedValue = await cacheProvider.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedValue != null || (cacheAttr.CacheNullValues && await CacheContainsKey(cacheProvider, cacheKey)))
        {
            return cachedValue!;
        }

        // Execute handler
        var response = await next();

        // Cache the response if not null or if caching null values
        if (response != null || cacheAttr.CacheNullValues)
        {
            if (cacheAttr.SlidingExpiration.HasValue)
            {
                await cacheProvider.SetWithSlidingAsync(
                    cacheKey, 
                    response, 
                    TimeSpan.FromSeconds(cacheAttr.SlidingExpiration.Value), 
                    cancellationToken);
            }
            else
            {
                await cacheProvider.SetAsync(
                    cacheKey, 
                    response, 
                    TimeSpan.FromSeconds(cacheAttr.Duration), 
                    cancellationToken);
            }
        }

        return response;
    }

    private async Task HandleCacheInvalidation(TRequest request, Type requestType, CancellationToken cancellationToken)
    {
        var invalidateAttrs = requestType.GetCustomAttributes(typeof(InvalidateCacheAttribute), true)
            .Cast<InvalidateCacheAttribute>()
            .ToArray();

        if (invalidateAttrs.Length == 0)
        {
            return;
        }

        var cacheProvider = _serviceProvider.GetService<ICacheProvider>();
        if (cacheProvider == null)
        {
            return;
        }

        foreach (var attr in invalidateAttrs)
        {
            foreach (var keyPattern in attr.Keys)
            {
                var actualKey = ResolveKeyPattern(request, keyPattern);
                
                if (attr.UsePattern)
                {
                    await cacheProvider.RemoveByPatternAsync(actualKey, cancellationToken);
                }
                else
                {
                    await cacheProvider.RemoveAsync(actualKey, cancellationToken);
                }
            }
        }
    }

    private string GenerateCacheKey(TRequest request, Type requestType, string? keyPattern)
    {
        if (!string.IsNullOrEmpty(keyPattern))
        {
            return ResolveKeyPattern(request, keyPattern);
        }

        // Default: use type name + property values
        var properties = requestType.GetProperties()
            .Where(p => p.CanRead)
            .Select(p => $"{p.Name}:{p.GetValue(request)}")
            .ToArray();

        return $"{requestType.Name}:{string.Join(":", properties)}";
    }

    private string ResolveKeyPattern(TRequest request, string pattern)
    {
        var result = pattern;
        var regex = new Regex(@"\{(\w+)\}");
        var matches = regex.Matches(pattern);

        foreach (Match match in matches)
        {
            var propertyName = match.Groups[1].Value;
            var property = request.GetType().GetProperty(propertyName);
            
            if (property != null)
            {
                var value = property.GetValue(request);
                result = result.Replace(match.Value, value?.ToString() ?? "null");
            }
        }

        return result;
    }

    private async Task<bool> CacheContainsKey(ICacheProvider cacheProvider, string key)
    {
        try
        {
            var value = await cacheProvider.GetAsync<object>(key);
            return value != null;
        }
        catch
        {
            return false;
        }
    }
}
