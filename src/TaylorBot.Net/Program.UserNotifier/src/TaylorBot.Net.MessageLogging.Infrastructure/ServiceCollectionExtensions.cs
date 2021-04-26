using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.MessageLogging.Domain;
using TaylorBot.Net.MessageLogging.Domain.DiscordEmbed;
using TaylorBot.Net.MessageLogging.Domain.Options;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Infrastructure
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddMessageLogging(this IConfigurationBuilder builder, IHostEnvironment environment)
        {
            return builder
                .AddJsonFile(path: "Settings/messageLog.json", optional: false, reloadOnChange: true)
                .AddJsonFile(path: $"Settings/messageLog.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageLoggingInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .ConfigureRequired<MessageDeletedLoggingOptions>(configuration, "MessageDeleted")
                .AddTransient<MessageLoggingChannelPostgresRepository>()
                .AddTransient<MessageLoggingRedisCacheRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<MessageDeletedLoggingOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<MessageLoggingRedisCacheRepository>() :
                        (IMessageLoggingChannelRepository)provider.GetRequiredService<MessageLoggingChannelPostgresRepository>();
                })
                .AddTransient<MessageLogChannelFinder>()
                .AddTransient<MessageDeletedEmbedFactory>()
                .AddSingleton<CachedMessageInMemoryRepository>()
                .AddTransient<CachedMessageRedisRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<MessageDeletedLoggingOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<CachedMessageRedisRepository>() :
                        (ICachedMessageRepository)provider.GetRequiredService<CachedMessageInMemoryRepository>();
                })
                .AddTransient<MessageDeletedLoggerService>();
        }
    }
}
