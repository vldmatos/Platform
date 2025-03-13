using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Configurations.Extensions;

public static class API
{
    public const string FixedWindow = "FixedWindow";

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