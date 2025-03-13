using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Threading.RateLimiting;

namespace Configurations.Extensions;

public interface IEndpoint
{
    string Group { get; }

    void MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder);
}

public static class API
{
    public const string FixedWindow = "FixedWindow";

    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] endpointServiceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(endpointServiceDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication webApplication, RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = webApplication.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder endpointRouteBuilder = routeGroupBuilder is null ? webApplication : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoints(endpointRouteBuilder);
        }

        return webApplication;
    }

    public static IServiceCollection AddRateLimits(this IServiceCollection services)
    {
        services.AddRateLimiter(limiters =>
        {
            limiters.AddFixedWindowLimiter(FixedWindow, options =>
            {
                options.PermitLimit = 2;
                options.Window = TimeSpan.FromSeconds(5);
                options.QueueLimit = 10;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            limiters.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}