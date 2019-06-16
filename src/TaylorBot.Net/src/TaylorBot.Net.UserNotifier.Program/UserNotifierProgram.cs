using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.UserNotifier.Program.Events;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.BirthdayReward.Infrastructure;
using TaylorBot.Net.BirthdayReward.Domain.Options;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.BirthdayReward.Domain.DiscordEmbed;
using TaylorBot.Net.Reminder.Domain.Options;
using TaylorBot.Net.Reminder.Domain;
using TaylorBot.Net.Reminder.Domain.DiscordEmbed;
using TaylorBot.Net.Reminder.Infrastructure;

namespace TaylorBot.Net.UserNotifier.Program
{
    public class UserNotifierProgram
    {
        public static async Task Main()
        {
            using (var services = new UserNotifierProgram().ConfigureServices())
            {
                await new TaylorBotApplication(services).StartAsync();
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
                .Build();

            return new ServiceCollection()
                .AddTaylorBotApplicationServices(config)
                .AddTaylorBotApplicationLogging(config)
                .ConfigureDatabaseConnection(config)
                .ConfigureRequired<BirthdayRewardNotifierOptions>(config, "BirthdayRewardNotifier")
                .ConfigureRequired<ReminderNotifierOptions>(config, "ReminderNotifier")
                .AddTransient<IAllReadyHandler, ReadyHandler>()
                .AddTransient<SingletonTaskRunner>()
                .AddTransient<IBirthdayRepository, BirthdayRepository>()
                .AddTransient<BirthdayRewardNotifierDomainService>()
                .AddTransient<BirthdayRewardEmbedFactory>()
                .AddTransient<IReminderRepository, ReminderRepository>()
                .AddTransient<ReminderNotifierDomainService>()
                .AddTransient<ReminderEmbedFactory>()
                .BuildServiceProvider();
        }
    }
}
