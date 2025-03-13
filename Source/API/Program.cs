using Configurations.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
       .AddAzureCosmosClient();

builder.Services.AddRateLimits()
                .AddOpenApi()
                .AddEndpoints(Assembly.GetExecutingAssembly());

var application = builder.Build();

application.MapDefaultEndpoints()
           .MapOpenApi();

application.MapEndpoints();

application.UseHttpsRedirection()
           .UseRateLimiter();

application.Run();


