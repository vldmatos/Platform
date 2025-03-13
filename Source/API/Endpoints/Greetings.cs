using Configurations.Extensions;

namespace API.Endpoints;
public class Greetings : IEndpoint
{
    public string Group => "Greetings";

    public void MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/greetings",
        async context =>
        {
            await context.Response.WriteAsync("Hi!");
        });
    }
}