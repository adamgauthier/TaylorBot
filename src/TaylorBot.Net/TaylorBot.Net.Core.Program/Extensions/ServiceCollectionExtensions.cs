using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Program.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Configuration;
using Discord.Rest;

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
                .AddTransient(provider => new DiscordSocketConfig { TotalShards = (int)provider.GetRequiredService<IOptionsMonitor<DiscordOptions>>().CurrentValue.ShardCount })
                .AddTransient(provider => new DiscordShardedClient(provider.GetRequiredService<DiscordSocketConfig>()))
                .AddSingleton<TaylorBotClient>();
        }

        public static IServiceCollection AddTaylorBotApplicationLogging(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddLogging(configure => configure.AddConsole().AddConfiguration(configuration.GetSection("Logging")));
        }
    }
}
