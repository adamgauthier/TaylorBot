using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.StatsTracker.Program.Events;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.MinutesTracker.Domain.Options;
using TaylorBot.Net.MinutesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Infrastructure;
using TaylorBot.Net.MessagesTracker.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.StatsTracker.Program
{
    public class StatsTrackerProgram
    {
        public static async Task Main()
        {
            using (var services = new StatsTrackerProgram().ConfigureServices())
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
                .AddJsonFile(path: $"Settings/minutesTracker.{environment}.json", optional: false)
                .Build();

            return new ServiceCollection()
                .AddTaylorBotApplicationServices(config)
                .AddTaylorBotApplicationLogging(config)
                .ConfigureDatabaseConnection(config)
                .ConfigureRequired<MinutesTrackerOptions>(config, "MinutesTracker")
                .AddTransient<IShardReadyHandler, ReadyHandler>()
                .AddTransient<IUserMessageReceivedHandler, UserMessageReceivedHandler>()
                .AddTransient<SingletonTaskRunner>()
                .AddTransient<IMinuteRepository, MinutesRepository>()
                .AddTransient<MinutesTrackerDomainService>()
                .AddTransient<IMessageRepository, MessagesRepository>()
                .AddTransient<WordCounter>()
                .AddTransient<MessagesTrackerDomainService>()
                .BuildServiceProvider();
        }
    }
}
