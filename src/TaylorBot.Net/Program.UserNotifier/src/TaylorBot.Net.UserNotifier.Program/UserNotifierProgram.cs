using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
using TaylorBot.Net.Reminder.Domain;
using TaylorBot.Net.Reminder.Domain.DiscordEmbed;
using TaylorBot.Net.Reminder.Domain.Options;
using TaylorBot.Net.Reminder.Infrastructure;
using TaylorBot.Net.UserNotifier.Program.Events;

namespace TaylorBot.Net.UserNotifier.Program
{
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
                        .AddJsonFile(path: $"Settings/patreonSync.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

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
                        .AddTransient<IAllReadyHandler, ReadyHandler>()
                        .AddTransient<IGuildUserLeftHandler, GuildUserLeftHandler>()
                        .AddTransient<IGuildUserBannedHandler, GuildUserBanHandler>()
                        .AddTransient<IGuildUserUnbannedHandler, GuildUserBanHandler>()
                        .AddTransient<IMessageDeletedHandler, MessageDeletedHandler>()
                        .AddTransient<IMessageBulkDeletedHandler, MessageBulkDeletedHandler>()
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
}
