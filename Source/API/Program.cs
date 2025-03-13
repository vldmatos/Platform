using Configurations.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
       .AddAzureCosmosClient();

builder.Services.AddRateLimits();



var application = builder.Build();

application.MapDefaultEndpoints()
           .MapOpenApi();

application.MapScalarApiReference();

application.UseHttpsRedirection()
           .UseRateLimiter();

application.Run();
