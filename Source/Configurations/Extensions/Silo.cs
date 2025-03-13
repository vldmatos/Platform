using Microsoft.Extensions.Hosting;

namespace Configurations.Extensions;

public static class Silo
{
    public static IHostApplicationBuilder ConfigureStorages(this IHostApplicationBuilder hostBuilder)
    {
        hostBuilder.UseOrleans(siloBuilder =>
        {
            ReadOnlySpan<string> storages = new[]
            {
                "company"
            };

            foreach (string storage in storages)
            {
                siloBuilder.AddCosmosGrainStorage(storage, configuration =>
                {
                    configuration.ConfigureCosmosClient("cosmos");
                    configuration.DatabaseName = "grains";
                    configuration.ContainerName = storage;
                    configuration.DatabaseThroughput = 300;
                    configuration.IsResourceCreationEnabled = true;
                });
            }
        });

        return hostBuilder;
    }
}
