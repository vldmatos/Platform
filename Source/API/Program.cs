using Configurations.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
       .AddAzureCosmosClient();

var application = builder.Build();

application.MapDefaultEndpoints();

if (application.Environment.IsDevelopment())
{
    application.MapOpenApi();
    application.MapScalarApiReference();
}

application.UseHttpsRedirection();

application.MapGet("/hi", () =>
{
    return "hi";
})
.WithName("GetWeatherForecast");

application.Run();
