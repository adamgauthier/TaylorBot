using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.BirthdayReward.Domain.DiscordEmbed;
using TaylorBot.Net.BirthdayReward.Domain.Options;
using TaylorBot.Net.BirthdayReward.Infrastructure;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.MemberLogging.Domain;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;
using TaylorBot.Net.MemberLogging.Domain.Options;
using TaylorBot.Net.MemberLogging.Infrastructure;
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
            using (var services = new UserNotifierProgram().ConfigureServices())
            {
                await new TaylorBotHostedService(services).StartAsync();
            }
        }

        private ServiceProvider ConfigureServices()
        {
            var environment = TaylorBotEnvironment.CreateCurrent();

            var config = new ConfigurationBuilder()
                .AddTaylorBotApplicationConfiguration(environment)
                .AddDatabaseConnectionConfiguration(environment)
                .AddJsonFile(path: $"Settings/birthdayRewardNotifier.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/reminderNotifier.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/memberLog.{environment}.json", optional: false)
                .Build();

            return new ServiceCollection()
                .AddTaylorBotApplicationServices(config)
                .AddTaylorBotApplicationLogging(config)
                .ConfigureDatabaseConnection(config)
                .ConfigureRequired<BirthdayRewardNotifierOptions>(config, "BirthdayRewardNotifier")
                .ConfigureRequired<ReminderNotifierOptions>(config, "ReminderNotifier")
                .ConfigureRequired<MemberLeftLoggingOptions>(config, "MemberLeft")
                .ConfigureRequired<MemberBanLoggingOptions>(config, "MemberBan")
                .AddTransient<IAllReadyHandler, ReadyHandler>()
                .AddTransient<IGuildUserLeftHandler, GuildUserLeftHandler>()
                .AddTransient<IGuildUserBannedHandler, GuildUserBanHandler>()
                .AddTransient<IGuildUserUnbannedHandler, GuildUserBanHandler>()
                .AddTransient<SingletonTaskRunner>()
                .AddTransient<IBirthdayRepository, BirthdayRepository>()
                .AddTransient<BirthdayRewardNotifierDomainService>()
                .AddTransient<BirthdayRewardEmbedFactory>()
                .AddTransient<IReminderRepository, ReminderRepository>()
                .AddTransient<ReminderNotifierDomainService>()
                .AddTransient<ReminderEmbedFactory>()
                .AddTransient<ILoggingTextChannelRepository, LoggingTextChannelRepository>()
                .AddTransient<MemberLogChannelFinder>()
                .AddTransient<GuildMemberLeftEmbedFactory>()
                .AddTransient<GuildMemberLeftLoggerService>()
                .AddTransient<GuildMemberBanEmbedFactory>()
                .AddTransient<GuildMemberBanLoggerService>()
                .BuildServiceProvider();
        }
    }
}
