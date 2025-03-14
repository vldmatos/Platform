using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Threading.RateLimiting;

namespace Configurations.Extensions;

public static class API
{
    public const string FixedWindow = "FixedWindow";

    public static IServiceCollection AddProblemDetail(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
         {
             options.CustomizeProblemDetails = context =>
             {
                 context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                 context.ProblemDetails.Extensions.TryAdd("request-id", context.HttpContext.TraceIdentifier);

                 var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                 context.ProblemDetails.Extensions.TryAdd("trace-id", activity?.Id);
             };
         });

        return services;
    }

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

public interface IEndpoint
{
    string Group { get; }

    void MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder);
}

public class ProblemException(string error, string message) : Exception(message)
{
    public string Error { get; } = error;

    public override string Message { get; } = message;
}

public class ProblemExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ProblemException problemException)
        {
            return true;
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = problemException.Error,
            Detail = problemException.Message,
            Type = "Bad Request",

        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
    }
}