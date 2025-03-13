using Collector;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureBlobClient("grains-state");
builder.UseOrleans();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
