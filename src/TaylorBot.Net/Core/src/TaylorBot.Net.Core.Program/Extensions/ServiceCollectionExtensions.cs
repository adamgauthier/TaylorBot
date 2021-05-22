using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Program.Options;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Program.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTaylorBotApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .ConfigureRequired<DiscordOptions>(configuration, "Discord")
                .AddTransient<ILogSeverityToLogLevelMapper, LogSeverityToLogLevelMapper>()
                .AddTransient<RawEventsHandler>()
                .AddTransient<DiscordRestClient>()
                .AddTransient(provider => new TaylorBotToken(provider.GetRequiredService<IOptionsMonitor<DiscordOptions>>().CurrentValue.Token))
                .AddSingleton(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<DiscordOptions>>().CurrentValue;

                    var config = new DiscordSocketConfig
                    {
                        ExclusiveBulkDelete = options.ExclusiveBulkDelete,
                    };

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

                    if (config.GatewayIntents == null)
                        throw new InvalidOperationException($"Creating client without {nameof(config.GatewayIntents)}.");

                    return new DiscordShardedClient(config);
                })
                .AddTransient<TaskExceptionLogger>()
                .AddSingleton<ITaylorBotClient, TaylorBotClient>()
                .AddTransient<Lazy<ITaylorBotClient>>(provider => new(() => provider.GetRequiredService<ITaylorBotClient>()));
        }
    }
}
