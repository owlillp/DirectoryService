using Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace FileService.Application.Messaging;

public static class WolverineConfiguration
{
    public static void AddWolverine(this WebApplicationBuilder builder)
    {
        string rabbitConnectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.RABBIT_MQ)!;
        string databaseConnectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.DATABASE)!;

        builder.Host.UseWolverine(
            options =>
        {
            options.ApplicationAssembly = typeof(WolverineConfiguration).Assembly;

            options.ConfigureDurableMessaging(databaseConnectionString);
            options.ConfigureRabbitMq(rabbitConnectionString);
            options.ConfigureStandardErrorPolicies();
        }, ExtensionDiscovery.ManualOnly);
    }

    private static void ConfigureDurableMessaging(this WolverineOptions options, string connectionString)
    {
        options.PersistMessagesWithPostgresql(connectionString, "public");
        options.UseEntityFrameworkCoreTransactions();
        options.Policies.UseDurableOutboxOnAllSendingEndpoints();
        options.Policies.UseDurableInboxOnAllListeners();
    }
}