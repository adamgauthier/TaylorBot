using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.EntityTracker.Domain.Guild;
using TaylorBot.Net.EntityTracker.Domain.GuildName;
using TaylorBot.Net.EntityTracker.Domain.Member;
using TaylorBot.Net.EntityTracker.Domain.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.EntityTracker.Domain.User;
using TaylorBot.Net.EntityTracker.Domain.Username;
using TaylorBot.Net.EntityTracker.Infrastructure.Guild;
using TaylorBot.Net.EntityTracker.Infrastructure.GuildName;
using TaylorBot.Net.EntityTracker.Infrastructure.Member;
using TaylorBot.Net.EntityTracker.Infrastructure.TextChannel;
using TaylorBot.Net.EntityTracker.Infrastructure.User;
using TaylorBot.Net.EntityTracker.Infrastructure.Username;

namespace TaylorBot.Net.EntityTracker.Infrastructure;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddEntityTracker(this IConfigurationBuilder builder, IHostEnvironment environment)
    {
        return builder
            .AddJsonFile(path: "Settings/entityTracker.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: $"Settings/entityTracker.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityTrackerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .ConfigureRequired<EntityTrackerOptions>(configuration, "EntityTracker")
            .AddTransient<UsernameTrackerDomainService>()
            .AddTransient<IUsernameRepository, UsernameRepository>()
            .AddTransient<GuildTrackerDomainService>()
            .AddTransient<EntityTrackerDomainService>()
            .AddTransient<IUserRepository, UserRepository>()
            .AddTransient<SpamChannelPostgresRepository>()
            .AddTransient<SpamChannelRedisCacheRepository>()
            .AddTransient(provider =>
            {
                var options = provider.GetRequiredService<IOptionsMonitor<EntityTrackerOptions>>().CurrentValue;
                return options.UseRedisCache ?
                    provider.GetRequiredService<SpamChannelRedisCacheRepository>() :
                    (ISpamChannelRepository)provider.GetRequiredService<SpamChannelPostgresRepository>();
            })
            .AddTransient<IGuildRepository, GuildRepository>()
            .AddTransient<IGuildNameRepository, GuildNameRepository>()
            .AddTransient<IMemberRepository, MemberRepository>();
    }
}
