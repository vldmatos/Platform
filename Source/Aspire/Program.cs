#pragma warning disable ASPIRECOSMOSDB001

var builder = DistributedApplication.CreateBuilder(args);

//var ollama = builder.AddOllama("ollama")
//                    .WithGPUSupport()
//                    .WithDataVolume()
//                    .WithOpenWebUI()
//                    .WithLifetime(ContainerLifetime.Persistent)
//                    .AddModel("phi3.5");

var cosmos = builder.AddAzureCosmosDB("cosmos").RunAsPreviewEmulator(emulator =>
{
    emulator.WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume()
            .WithDataExplorer();
})
.AddCosmosDatabase("grains");

var storage = builder.AddAzureStorage("storage").RunAsEmulator();

var clusteringTable = storage.AddTables("clustering");
var grainStorage = storage.AddBlobs("grains-state");

var orleans = builder.AddOrleans("cluster")
                     .WithClustering(clusteringTable)
                     .WithGrainStorage("grains", grainStorage);

builder.AddProject<Projects.Collector>("collector")
       .WithReference(orleans)
       .WithReplicas(10);

builder.AddProject<Projects.API>("api")
       .WithReference(cosmos)
       .WaitFor(cosmos);

builder.Build().Run();
