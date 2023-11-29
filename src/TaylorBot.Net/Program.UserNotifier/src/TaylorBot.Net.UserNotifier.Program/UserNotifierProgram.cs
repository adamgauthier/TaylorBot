using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using DontPanic.TumblrSharp.OAuth;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Reddit;
using System.Net.Http.Headers;
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
using TaylorBot.Net.InstagramNotifier.Domain;
using TaylorBot.Net.InstagramNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.InstagramNotifier.Domain.Options;
using TaylorBot.Net.InstagramNotifier.Infrastructure;
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

public class UserNotifierProgram
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
                    .AddJsonFile(path: "Settings/instagramNotifier.json", optional: false, reloadOnChange: true);

                appConfig.AddEnvironmentVariables("TaylorBot_");
            })
            .ConfigureServices((hostBuilderContext, services) =>
            {
                var config = hostBuilderContext.Configuration;
                services
                    .AddHostedService<TaylorBotHostedService>()
                    .AddTaylorBotApplicationServices(config)
                    .AddPostgresConnection(config)
                    .AddRedisConnection(config)
                    .AddMessageLoggingInfrastructure(config)
                    .ConfigureRequired<BirthdayRewardNotifierOptions>(config, "BirthdayRewardNotifier")
                    .ConfigureRequired<ReminderNotifierOptions>(config, "ReminderNotifier")
                    .ConfigureRequired<MemberLeftLoggingOptions>(config, "MemberLeft")
                    .ConfigureRequired<MemberBanLoggingOptions>(config, "MemberBan")
                    .ConfigureRequired<PatreonSyncOptions>(config, "PatreonSync")
                    .AddTransient<IShardReadyHandler, ShardReadyHandler>()
                    .AddTransient<IAllReadyHandler, ReadyHandler>()
                    .AddTransient<IGuildUserLeftHandler, GuildUserLeftHandler>()
                    .AddTransient<IGuildUserBannedHandler, GuildUserBanHandler>()
                    .AddTransient<IGuildUserUnbannedHandler, GuildUserBanHandler>()
                    .AddTransient<IMessageDeletedHandler, MessageDeletedHandler>()
                    .AddTransient<IMessageUpdatedHandler, MessageUpdatedHandler>()
                    .AddTransient<IMessageBulkDeletedHandler, MessageBulkDeletedHandler>()
                    .AddTransient<IMessageReceivedHandler, MessageReceivedHandler>()
                    .AddTransient<IReactionRemovedHandler, ReactionRemovedHandler>()
                    .AddTransient<SingletonTaskRunner>()
                    .AddTransient<IBirthdayRepository, BirthdayRepository>()
                    .AddTransient<BirthdayRewardNotifierDomainService>()
                    .AddTransient<BirthdayRewardEmbedFactory>()
                    .AddTransient<IReminderRepository, ReminderRepository>()
                    .AddTransient<ReminderNotifierDomainService>()
                    .AddTransient<ReminderEmbedFactory>()
                    .AddTransient<IMemberLoggingChannelRepository, MemberLoggingChannelRepository>()
                    .AddTransient<MemberLogChannelFinder>()
                    .AddTransient<GuildMemberLeftEmbedFactory>()
                    .AddTransient<GuildMemberLeftLoggerService>()
                    .AddTransient<GuildMemberBanEmbedFactory>()
                    .AddTransient<GuildMemberBanLoggerService>()
                    .AddTransient<IPlusRepository, PlusPostgresRepository>()
                    .AddTransient<PatreonSyncDomainService>()
                    .ConfigureRequired<RedditNotifierOptions>(config, "RedditNotifier")
                    .ConfigureRequired<RedditAuthOptions>(config, "RedditAuth")
                    .AddSingleton(provider =>
                    {
                        var auth = provider.GetRequiredService<IOptionsMonitor<RedditAuthOptions>>().CurrentValue;
                        return new RedditClient(appId: auth.AppId, appSecret: auth.AppSecret, refreshToken: auth.RefreshToken);
                    })
                    .ConfigureRequired<YoutubeNotifierOptions>(config, "YoutubeNotifier")
                    .ConfigureRequired<YoutubeAuthOptions>(config, "YoutubeAuth")
                    .AddSingleton(provider =>
                    {
                        var auth = provider.GetRequiredService<IOptionsMonitor<YoutubeAuthOptions>>().CurrentValue;
                        return new BaseClientService.Initializer() { ApiKey = auth.ApiKey };
                    })
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
                    .ConfigureRequired<InstagramNotifierOptions>(config, "InstagramNotifier")
                    .AddTransient<IRedditCheckerRepository, RedditCheckerRepository>()
                    .AddTransient<RedditNotifierService>()
                    .AddTransient<RedditPostToEmbedMapper>()
                    .AddTransient<YouTubeService>()
                    .AddTransient<IYoutubeCheckerRepository, YoutubeCheckerRepository>()
                    .AddTransient<YoutubeNotifierService>()
                    .AddTransient<YoutubePostToEmbedMapper>()
                    .AddTransient<ITumblrCheckerRepository, TumblrCheckerRepository>()
                    .AddTransient<TumblrNotifierService>()
                    .AddTransient<TumblrPostToEmbedMapper>()
                    .AddTransient<IInstagramCheckerRepository, InstagramCheckerPostgresRepository>()
                    .AddTransient<IInstagramClient, InstagramRestClient>()
                    .AddTransient<InstagramNotifierService>()
                    .AddTransient<InstagramPostToEmbedMapper>()
                    .AddHttpClient<IPatreonClient, PatreonHttpClient>((provider, client) =>
                    {
                        var options = provider.GetRequiredService<IOptionsMonitor<PatreonSyncOptions>>().CurrentValue;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
                    });
            })
            .Build();

        await host.RunAsync();
    }
}
