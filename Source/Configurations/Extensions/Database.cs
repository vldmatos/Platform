using Microsoft.Extensions.Hosting;

namespace Configurations.Extensions;

public static class Database
{
    public static IHostApplicationBuilder AddAzureCosmosClient(this IHostApplicationBuilder builder)
    {
        builder.AddAzureCosmosClient("cosmos");

        return builder;
    }
}
