using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Application.Configuration;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTaylorBotApplicationServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ILogSeverityToLogLevelMapper, LogSeverityToLogLevelMapper>()
                .AddTransient<ITokenProvider, TaylorBotConfiguration>()
                .AddTransient<IShardCountProvider, TaylorBotConfiguration>()
                .AddTransient(provider => new DiscordSocketConfig { TotalShards = provider.GetRequiredService<IShardCountProvider>().GetShardCount() })
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
