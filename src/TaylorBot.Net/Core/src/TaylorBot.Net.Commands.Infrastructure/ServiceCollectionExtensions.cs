using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Infrastructure.Options;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.EntityTracker.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddCommandClient(this IConfigurationBuilder builder, IHostEnvironment environment)
        {
            return builder
                .AddEntityTracker()
                .AddJsonFile(path: "Settings/commandClient.json", optional: false, reloadOnChange: true)
                .AddJsonFile(path: $"Settings/commandClient.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddEntityTrackerInfrastructure(configuration)
                .ConfigureRequired<CommandClientOptions>(configuration, "CommandClient")
                .AddTransient<CommandPrefixPostgresRepository>()
                .AddTransient<CommandPrefixRedisCacheRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<CommandPrefixRedisCacheRepository>() :
                        (ICommandPrefixRepository)provider.GetRequiredService<CommandPrefixPostgresRepository>();
                })
                .AddTransient<DisabledCommandPostgresRepository>()
                .AddTransient<DisabledCommandRedisCacheRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<DisabledCommandRedisCacheRepository>() :
                        (IDisabledCommandRepository)provider.GetRequiredService<DisabledCommandPostgresRepository>();
                })
                .AddTransient<DisabledGuildCommandPostgresRepository>()
                .AddTransient<DisabledGuildCommandRedisCacheRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<DisabledGuildCommandRedisCacheRepository>() :
                        (IDisabledGuildCommandRepository)provider.GetRequiredService<DisabledGuildCommandPostgresRepository>();
                })
                .AddTransient<DisabledGuildChannelCommandPostgresRepository>()
                .AddTransient<DisabledGuildChannelCommandRedisCacheRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<DisabledGuildChannelCommandRedisCacheRepository>() :
                        (IDisabledGuildChannelCommandRepository)provider.GetRequiredService<DisabledGuildChannelCommandPostgresRepository>();
                })
                .AddTransient<IgnoredUserPostgresRepository>()
                .AddTransient<IgnoredUserRedisCacheRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<IgnoredUserRedisCacheRepository>() :
                        (IIgnoredUserRepository)provider.GetRequiredService<IgnoredUserPostgresRepository>();
                })
                .AddTransient<MemberPostgresRepository>()
                .AddTransient<MemberRedisCacheRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<MemberRedisCacheRepository>() :
                        (IMemberRepository)provider.GetRequiredService<MemberPostgresRepository>();
                })
                .AddSingleton<OnGoingCommandInMemoryRepository>()
                .AddSingleton<OnGoingCommandRedisRepository>()
                .AddTransient(provider =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                    return options.UseRedisCache ?
                        provider.GetRequiredService<OnGoingCommandRedisRepository>() :
                        (IOngoingCommandRepository)provider.GetRequiredService<OnGoingCommandInMemoryRepository>();
                })
                .AddSingleton<ICommandUsageRepository, CommandUsagePostgresRepository>();
        }
    }
}
