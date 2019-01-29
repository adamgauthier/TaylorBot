using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Application.Configuration;
using TaylorBot.Net.Core;
using TaylorBot.Net.Core.Configuration;

namespace TaylorBot.Net.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultTaylorBotServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ITokenProvider, TaylorBotConfiguration>()
                .AddTransient<IShardCountProvider, TaylorBotConfiguration>()
                .AddTransient(provider => new DiscordSocketConfig { TotalShards = provider.GetRequiredService<IShardCountProvider>().GetShardCount() })
                .AddTransient(provider => new DiscordShardedClient(provider.GetRequiredService<DiscordSocketConfig>()))
                .AddSingleton<TaylorBotClient>();
        }
    }
}
