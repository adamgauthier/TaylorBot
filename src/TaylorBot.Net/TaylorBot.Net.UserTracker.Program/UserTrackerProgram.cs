using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.UserTracker.Program.Events;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.UsernameTracker.Domain;
using TaylorBot.Net.UsernameTracker.Infrastructure;
using TaylorBot.Net.QuickStart.Domain.Options;
using TaylorBot.Net.QuickStart.Domain;

namespace TaylorBot.Net.UserTracker.Program
{
    public class UserTrackerProgram
    {
        public static async Task Main()
        {
            using (var services = new UserTrackerProgram().ConfigureServices())
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
                .AddJsonFile(path: $"Settings/quickStartEmbed.{environment}.json", optional: false)
                .Build();

            return new ServiceCollection()
                .AddTaylorBotApplicationServices(config)
                .AddTaylorBotApplicationLogging(config)
                .ConfigureDatabaseConnection(config)
                .ConfigureRequired<QuickStartEmbedOptions>(config, "QuickStartEmbed")
                .AddTransient<IJoinedGuildHandler, JoinedGuildHandler>()
                .AddTransient<IUserUpdatedHandler, UserUpdatedHandler>()
                .AddTransient<QuickStartDomainService>()
                .AddTransient<UsernameTrackerDomainService>()
                .AddTransient<IUsernameRepository, UsernameRepository>()
                .BuildServiceProvider();
        }
    }
}
