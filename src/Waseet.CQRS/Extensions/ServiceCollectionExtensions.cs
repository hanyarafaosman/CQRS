using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Waseet.CQRS.Extensions;

/// <summary>
/// Extension methods for configuring Waseet CQRS services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Waseet CQRS services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="assemblies">Assemblies to scan for handlers.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddWaseet(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register the mediator
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IPublisher>(sp => sp.GetRequiredService<IMediator>());

        // If no assemblies provided, use the calling assembly
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Register pipeline behaviors (order matters)
        // 1. Authorization (security first)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Authorization.AuthorizationBehavior<,>));
        // 2. Idempotency (check if already processed)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Idempotency.IdempotencyBehavior<,>));
        // 3. Caching (check cache after auth & idempotency)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Caching.CacheBehavior<,>));
        // 4. Performance Monitoring (wraps everything)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Monitoring.PerformanceMonitoringBehavior<,>));
        // 5. Auditing (log the request)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Auditing.AuditBehavior<,>));
        // 6. Validation (validate before handler)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Validation.ValidationBehavior<,>));

        // Register all request handlers
        RegisterHandlers(services, assemblies);
        
        // Register all notification handlers
        RegisterNotificationHandlers(services, assemblies);

        // Register all stream handlers
        RegisterStreamHandlers(services, assemblies);
        
        // Register all validators
        RegisterValidators(services, assemblies);

        return services;
    }

    /// <summary>
    /// Adds Waseet CQRS services to the specified IServiceCollection using a configuration action.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The configuration action.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddWaseet(this IServiceCollection services, Action<WaseetConfiguration> configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var config = new WaseetConfiguration();
        configuration(config);

        // Register the mediator
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IPublisher>(sp => sp.GetRequiredService<IMediator>());

        // Register all request handlers
        RegisterHandlers(services, config.Assemblies.ToArray());
        
        // Register all notification handlers
        RegisterNotificationHandlers(services, config.Assemblies.ToArray());

        // Register all stream handlers
        RegisterStreamHandlers(services, config.Assemblies.ToArray());

        return services;
    }

    /// <summary>
    /// Adds validation support with automatic validation behavior registration.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="assemblies">Assemblies to scan for validators.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddWaseetValidation(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register validation behavior for all request types
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Validation.ValidationBehavior<,>));

        // If no assemblies provided, use the calling assembly
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Register all validators
        RegisterValidators(services, assemblies);

        return services;
    }

    /// <summary>
    /// Adds caching support for CQRS requests.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="cacheProvider">Optional custom cache provider. If not specified, uses MemoryCacheProvider.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddWaseetCaching(this IServiceCollection services, Func<IServiceProvider, Caching.ICacheProvider>? cacheProvider = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register memory cache
        services.AddMemoryCache();

        // Register cache provider
        if (cacheProvider != null)
        {
            services.AddSingleton(cacheProvider);
        }
        else
        {
            services.AddSingleton<Caching.ICacheProvider, Caching.MemoryCacheProvider>();
        }

        return services;
    }

    /// <summary>
    /// Adds performance monitoring for CQRS requests.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="performanceMonitor">Optional custom performance monitor. If not specified, uses InMemoryPerformanceMonitor.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddWaseetMonitoring(this IServiceCollection services, Func<IServiceProvider, Monitoring.IPerformanceMonitor>? performanceMonitor = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register performance monitor
        if (performanceMonitor != null)
        {
            services.AddSingleton(performanceMonitor);
        }
        else
        {
            services.AddSingleton<Monitoring.IPerformanceMonitor, Monitoring.InMemoryPerformanceMonitor>();
        }

        return services;
    }

    /// <summary>
    /// Adds audit logging for CQRS requests.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="auditLogger">Optional custom audit logger. If not specified, uses ConsoleAuditLogger.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddWaseetAuditing(this IServiceCollection services, Func<IServiceProvider, Auditing.IAuditLogger>? auditLogger = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register audit logger
        if (auditLogger != null)
        {
            services.AddSingleton(auditLogger);
        }
        else
        {
            services.AddSingleton<Auditing.IAuditLogger, Auditing.ConsoleAuditLogger>();
        }

        return services;
    }

    /// <summary>
    /// Adds Elasticsearch audit logging for CQRS requests.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="elasticsearchUrl">Elasticsearch URL (e.g., http://localhost:9200)</param>
    /// <param name="indexPrefix">Index prefix (default: "waseet-audit")</param>
    /// <param name="username">Optional username for authentication</param>
    /// <param name="password">Optional password for authentication</param>
    /// <param name="ignoreSslErrors">Set to true to ignore SSL certificate errors (for development/self-signed certificates)</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddWaseetElasticsearchAuditing(
        this IServiceCollection services,
        string elasticsearchUrl,
        string indexPrefix = "waseet-audit",
        string? username = null,
        string? password = null,
        bool ignoreSslErrors = false)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<Auditing.IAuditLogger>(sp =>
            new Auditing.ElasticsearchAuditLogger(elasticsearchUrl, indexPrefix, username, password, ignoreSslErrors));

        return services;
    }

    /// <summary>
    /// Adds idempotency support for CQRS commands.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="idempotencyStore">Optional custom idempotency store. If not specified, uses MemoryIdempotencyStore.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddWaseetIdempotency(this IServiceCollection services, Func<IServiceProvider, Idempotency.IIdempotencyStore>? idempotencyStore = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register idempotency store
        if (idempotencyStore != null)
        {
            services.AddSingleton(idempotencyStore);
        }
        else
        {
            services.AddSingleton<Idempotency.IIdempotencyStore, Idempotency.MemoryIdempotencyStore>();
        }

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerInterfaceType = typeof(IRequestHandler<,>);

        foreach (var assembly in assemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces(), (type, interfaceType) => new { type, interfaceType })
                .Where(x => x.interfaceType.IsGenericType && 
                           x.interfaceType.GetGenericTypeDefinition() == handlerInterfaceType)
                .ToList();

            foreach (var handler in handlers)
            {
                services.AddTransient(handler.interfaceType, handler.type);
            }
        }
    }

    private static void RegisterNotificationHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerInterfaceType = typeof(INotificationHandler<>);

        foreach (var assembly in assemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces(), (type, interfaceType) => new { type, interfaceType })
                .Where(x => x.interfaceType.IsGenericType &&
                           x.interfaceType.GetGenericTypeDefinition() == handlerInterfaceType)
                .ToList();

            foreach (var handler in handlers)
            {
                services.AddTransient(handler.interfaceType, handler.type);
            }
        }
    }

    private static void RegisterValidators(IServiceCollection services, Assembly[] assemblies)
    {
        var validatorInterfaceType = typeof(Validation.IValidator<>);

        foreach (var assembly in assemblies)
        {
            var validators = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces(), (type, interfaceType) => new { type, interfaceType })
                .Where(x => x.interfaceType.IsGenericType &&
                           x.interfaceType.GetGenericTypeDefinition() == validatorInterfaceType)
                .ToList();

            foreach (var validator in validators)
            {
                services.AddTransient(validator.interfaceType, validator.type);
            }
        }
    }

    private static void RegisterStreamHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerInterfaceType = typeof(IStreamRequestHandler<,>);

        foreach (var assembly in assemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces(), (type, interfaceType) => new { type, interfaceType })
                .Where(x => x.interfaceType.IsGenericType &&
                           x.interfaceType.GetGenericTypeDefinition() == handlerInterfaceType)
                .ToList();

            foreach (var handler in handlers)
            {
                services.AddTransient(handler.interfaceType, handler.type);
            }
        }
    }
}

/// <summary>
/// Configuration for Waseet CQRS.
/// </summary>
public class WaseetConfiguration
{
    internal readonly List<Assembly> Assemblies = new();

    /// <summary>
    /// Register handlers from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for handlers.</param>
    /// <returns>The configuration for further configuration.</returns>
    public WaseetConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies != null)
        {
            Assemblies.AddRange(assemblies);
        }
        return this;
    }

    /// <summary>
    /// Register handlers from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly should be scanned.</typeparam>
    /// <returns>The configuration for further configuration.</returns>
    public WaseetConfiguration RegisterServicesFromAssemblyContaining<T>()
    {
        Assemblies.Add(typeof(T).Assembly);
        return this;
    }
}
