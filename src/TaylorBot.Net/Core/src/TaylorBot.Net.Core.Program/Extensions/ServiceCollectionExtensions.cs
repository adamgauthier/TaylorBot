using Azure.Monitor.OpenTelemetry.AspNetCore;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Program.Instrumentation;
using TaylorBot.Net.Core.Program.Options;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Program.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaylorBotApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<TaylorBotInstrumentation>();

        var builder = services
            .AddOpenTelemetry()
            .WithTracing(o => o.AddSource(TaylorBotInstrumentation.ActivitySourceName));

        var appInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        ArgumentException.ThrowIfNullOrEmpty(appInsightsConnectionString);

        if (!appInsightsConnectionString.Equals("no_application_insights", StringComparison.OrdinalIgnoreCase))
        {
            builder.UseAzureMonitor();
        }

        services.ConfigureOpenTelemetryMeterProvider(metrics => metrics
            // Remove noisy metrics that incur storage cost, only keep http.client.request.duration
            .AddView(instrumentName: "http.client.open_connections", MetricStreamConfiguration.Drop)
            .AddView(instrumentName: "http.client.active_requests", MetricStreamConfiguration.Drop)
            .AddView(instrumentName: "http.client.connection.duration", MetricStreamConfiguration.Drop)
            .AddView(instrumentName: "http.client.request.time_in_queue", MetricStreamConfiguration.Drop)
        );

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
