using Azure.Monitor.OpenTelemetry.AspNetCore;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Program.Instrumentation;
using TaylorBot.Net.Core.Program.Options;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Program.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaylorBotApplicationServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        var instrumentation = new TaylorBotInstrumentation(hostEnvironment.ApplicationName);

        services
            .AddSingleton(instrumentation)
            .AddOpenTelemetry()
            .ConfigureResource(b => b.AddAttributes([new KeyValuePair<string, object>("service.name", hostEnvironment.ApplicationName)]))
            .WithTracing(o => o.AddSource(instrumentation.ActivitySource.Name))
            .UseAzureMonitor();

        return services
            .ConfigureRequired<DiscordOptions>(configuration, "Discord")
            .AddTransient<ILogSeverityToLogLevelMapper, LogSeverityToLogLevelMapper>()
            .AddTransient<RawEventsHandler>()
            .AddTransient(provider => new TaylorBotToken(provider.GetRequiredService<IOptionsMonitor<DiscordOptions>>().CurrentValue.Token))
            .AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptionsMonitor<DiscordOptions>>().CurrentValue;

                var config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.None };

                if (options.ShardCount.HasValue)
                {
                    config.TotalShards = (int)options.ShardCount.Value;
                }

                if (options.MessageCacheSize.HasValue)
                {
                    config.MessageCacheSize = (int)options.MessageCacheSize.Value;
                }

                return config;
            })
            .AddTransient(provider =>
            {
                var config = provider.GetRequiredService<DiscordSocketConfig>();

                if (config.GatewayIntents == GatewayIntents.None)
                    throw new InvalidOperationException("Creating client no gateway intents.");

                return new DiscordShardedClient(config);
            })
            .AddTransient<TaskExceptionLogger>()
            .AddSingleton<ITaylorBotClient, TaylorBotClient>()
            .AddTransient<Lazy<ITaylorBotClient>>(provider => new(() => provider.GetRequiredService<ITaylorBotClient>()));
    }
}
