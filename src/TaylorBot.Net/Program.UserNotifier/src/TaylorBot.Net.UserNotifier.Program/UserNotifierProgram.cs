using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using DontPanic.TumblrSharp.OAuth;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.BirthdayReward.Domain.DiscordEmbed;
using TaylorBot.Net.BirthdayReward.Domain.Options;
using TaylorBot.Net.BirthdayReward.Infrastructure;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MemberLogging.Domain;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;
using TaylorBot.Net.MemberLogging.Domain.Options;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;
using TaylorBot.Net.MemberLogging.Infrastructure;
using TaylorBot.Net.MessageLogging.Infrastructure;
using TaylorBot.Net.PatreonSync.Domain;
using TaylorBot.Net.PatreonSync.Domain.Options;
using TaylorBot.Net.PatreonSync.Infrastructure;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.RedditNotifier.Domain.Options;
using TaylorBot.Net.RedditNotifier.Infrastructure;
using TaylorBot.Net.Reminder.Domain;
using TaylorBot.Net.Reminder.Domain.DiscordEmbed;
using TaylorBot.Net.Reminder.Domain.Options;
using TaylorBot.Net.Reminder.Infrastructure;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.TumblrNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.TumblrNotifier.Domain.Options;
using TaylorBot.Net.TumblrNotifier.Infrastructure;
using TaylorBot.Net.UserNotifier.Program.Events;
using TaylorBot.Net.YoutubeNotifier.Domain;
using TaylorBot.Net.YoutubeNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.YoutubeNotifier.Domain.Options;
using TaylorBot.Net.YoutubeNotifier.Infrastructure;

namespace TaylorBot.Net.UserNotifier.Program;

public sealed class UserNotifierProgram
{
    public static async Task Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostBuilderContext, appConfig) =>
            {
                var env = hostBuilderContext.HostingEnvironment;

                appConfig
                    .AddTaylorBotApplication(env)
                    .AddDatabaseConnection(env)
                    .AddRedisConnection(env)
                    .AddMessageLogging(env)
                    .AddJsonFile(path: "Settings/birthdayRewardNotifier.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: "Settings/reminderNotifier.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: "Settings/memberLog.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: "Settings/patreonSync.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: $"Settings/patreonSync.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile(path: "Settings/redditNotifier.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: "Settings/youtubeNotifier.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: "Settings/tumblrNotifier.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: "Settings/birthdayRole.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: $"Settings/birthdayRole.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    ;

                appConfig.AddEnvironmentVariables("TaylorBot_");
            })
            .ConfigureServices((hostBuilderContext, services) =>
            {
                var config = hostBuilderContext.Configuration;
                services
                    .AddHostedService<TaylorBotHostedService>()
                    .AddTaylorBotApplicationServices(config)
                    .AddPostgresConnection(config, withTracing: false)
                    .AddRedisConnection(config)
                    .AddMessageLogging(config)
                    .AddMemberLeaveLogging(config)
                    .AddBirthdayReward(config)
                    .AddBirthdayRole(config)
                    .AddPatreonSync(config)
                    .AddRedditNotify(config)
                    .AddTumblrNotify(config)
                    .AddYoutubeNotify(config)
                    .AddReminderNotify(config)
                    .AddBirthdayCalendarRefresh()
                    .AddTransient<SingletonTaskRunner>()
                    .AddTransient<IShardReadyHandler, ShardReadyHandler>()
                    .AddTransient<IGuildUserLeftHandler, GuildUserLeftHandler>()
                    .AddTransient<IGuildUserBannedHandler, GuildUserBanHandler>()
                    .AddTransient<IGuildUserUnbannedHandler, GuildUserBanHandler>()
                    .AddTransient<IMessageDeletedHandler, MessageDeletedHandler>()
                    .AddTransient<IMessageUpdatedHandler, MessageUpdatedHandler>()
                    .AddTransient<IMessageBulkDeletedHandler, MessageBulkDeletedHandler>()
                    .AddTransient<IMessageReceivedHandler, MessageReceivedHandler>()
                    .AddTransient<IReactionRemovedHandler, ReactionRemovedHandler>()
                    ;
            })
            .Build();

        await host.RunAsync();
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBirthdayCalendarRefresh(this IServiceCollection services)
    {
        return services
            .AddTransient<IBirthdayCalendarRepository, BirthdayCalendarPostgresRepository>()
            .AddTransient<BirthdayCalendarDomainService>();
    }

    public static IServiceCollection AddBirthdayReward(this IServiceCollection services, IConfiguration config)
    {
        return services
            .ConfigureRequired<BirthdayRewardNotifierOptions>(config, "BirthdayRewardNotifier")
            .AddTransient<BirthdayRewardNotifierDomainService>()
            .AddTransient<BirthdayRewardEmbedFactory>()
            .AddTransient<IBirthdayRepository, BirthdayPostgresRepository>();
    }

    public static IServiceCollection AddBirthdayRole(this IServiceCollection services, IConfiguration config)
    {
        return services
            .ConfigureRequired<BirthdayRoleOptions>(config, "BirthdayRole")
            .AddSingleton<IValidateOptions<BirthdayRoleOptions>, BirthdayRoleOptionsValidator>()
            .AddTransient<IBirthdayRoleRepository, BirthdayRolePostgresRepository>()
            .AddTransient<BirthdayRoleDomainService>();
    }

    public static IServiceCollection AddPatreonSync(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient<IPatreonClient, PatreonHttpClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<PatreonSyncOptions>>().CurrentValue;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });

        return services
            .ConfigureRequired<PatreonSyncOptions>(config, "PatreonSync")
            .AddTransient<IPlusRepository, PlusPostgresRepository>()
            .AddTransient<PatreonSyncDomainService>();
    }

    public static IServiceCollection AddRedditNotify(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient<RedditAuthHttpClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<RedditAuthOptions>>().CurrentValue;

            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.AppId}:{options.AppSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            client.DefaultRequestHeaders.UserAgent.ParseAdd("TaylorBot/1.0.0");
        });

        services.AddHttpClient<RedditHttpClient>((provider, client) =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("TaylorBot/1.0.0");
        });

        return services
            .ConfigureRequired<RedditNotifierOptions>(config, "RedditNotifier")
            .ConfigureRequired<RedditAuthOptions>(config, "RedditAuth")
            .AddSingleton<RedditTokenInMemoryRepository>()
            .AddTransient<IRedditCheckerRepository, RedditCheckerRepository>()
            .AddTransient<RedditNotifierService>()
            .AddTransient<RedditPostToEmbedMapper>();
    }

    public static IServiceCollection AddTumblrNotify(this IServiceCollection services, IConfiguration config)
    {
        return services
            .ConfigureRequired<TumblrNotifierOptions>(config, "TumblrNotifier")
            .ConfigureRequired<TumblrAuthOptions>(config, "TumblrAuth")
            .AddSingleton(provider =>
            {
                var auth = provider.GetRequiredService<IOptionsMonitor<TumblrAuthOptions>>().CurrentValue;
                return new TumblrClientFactory().Create<TumblrClient>(
                    consumerKey: auth.ConsumerKey,
                    consumerSecret: auth.ConsumerSecret,
                    new Token(key: auth.Token, secret: auth.TokenSecret)
                );
            })
            .AddTransient<ITumblrCheckerRepository, TumblrCheckerRepository>()
            .AddTransient<TumblrNotifierService>()
            .AddTransient<TumblrPostToEmbedMapper>();
    }

    public static IServiceCollection AddYoutubeNotify(this IServiceCollection services, IConfiguration config)
    {
        return services
            .ConfigureRequired<YoutubeNotifierOptions>(config, "YoutubeNotifier")
            .ConfigureRequired<YoutubeAuthOptions>(config, "YoutubeAuth")
            .AddSingleton(provider =>
            {
                var auth = provider.GetRequiredService<IOptionsMonitor<YoutubeAuthOptions>>().CurrentValue;
                return new BaseClientService.Initializer() { ApiKey = auth.ApiKey };
            })
            .AddTransient<YouTubeService>()
            .AddTransient<IYoutubeCheckerRepository, YoutubeCheckerPostgresRepository>()
            .AddTransient<YoutubeNotifierService>()
            .AddTransient<YoutubePostToEmbedMapper>();
    }

    public static IServiceCollection AddReminderNotify(this IServiceCollection services, IConfiguration config)
    {
        return services
            .ConfigureRequired<ReminderNotifierOptions>(config, "ReminderNotifier")
            .AddTransient<IReminderRepository, ReminderRepository>()
            .AddTransient<ReminderNotifierDomainService>()
            .AddTransient<ReminderEmbedFactory>();
    }

    public static IServiceCollection AddMemberLeaveLogging(this IServiceCollection services, IConfiguration config)
    {
        return services
            .ConfigureRequired<MemberLeftLoggingOptions>(config, "MemberLeft")
            .ConfigureRequired<MemberBanLoggingOptions>(config, "MemberBan")
            .AddTransient<IMemberLoggingChannelRepository, MemberLoggingChannelRepository>()
            .AddTransient<MemberLogChannelFinder>()
            .AddTransient<GuildMemberLeftEmbedFactory>()
            .AddTransient<GuildMemberLeftLoggerService>()
            .AddTransient<GuildMemberBanEmbedFactory>()
            .AddTransient<GuildMemberBanLoggerService>();
    }
}
