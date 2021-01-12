using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                .AddTransient<DiscordRestClient>()
                .AddTransient(provider => new TaylorBotToken(provider.GetRequiredService<IOptionsMonitor<DiscordOptions>>().CurrentValue.Token))
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<DiscordOptions>>().CurrentValue;

                    var config = new DiscordSocketConfig
                    {
                        TotalShards = (int)options.ShardCount,
                        ExclusiveBulkDelete = options.ExclusiveBulkDelete,
                        GatewayIntents =
                            GatewayIntents.Guilds |
                            GatewayIntents.GuildMembers |
                            GatewayIntents.GuildBans |
                            GatewayIntents.GuildMessages |
                            GatewayIntents.DirectMessages |
                            GatewayIntents.DirectMessageReactions |
                            GatewayIntents.GuildMessageReactions
                    };

                    if (options.MessageCacheSize.HasValue)
                    {
                        config.MessageCacheSize = (int)options.MessageCacheSize.Value;
                    }

                    return config;
                })
                .AddTransient(provider => new DiscordShardedClient(provider.GetRequiredService<DiscordSocketConfig>()))
                .AddTransient<TaskExceptionLogger>()
                .AddSingleton<ITaylorBotClient, TaylorBotClient>();
        }
    }
}
