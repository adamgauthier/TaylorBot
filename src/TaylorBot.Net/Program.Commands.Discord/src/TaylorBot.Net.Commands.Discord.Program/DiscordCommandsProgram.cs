using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Jail.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Infrastructure;
using TaylorBot.Net.Commands.Extensions;
using TaylorBot.Net.Commands.Infrastructure;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program.Extensions;

namespace TaylorBot.Net.Commands.Discord.Program
{
    public class DiscordCommandsProgram
    {
        public static async Task Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostBuilderContext, appConfig) =>
                {
                    appConfig.Sources.Clear();

                    var env = hostBuilderContext.HostingEnvironment;

                    appConfig
                        .AddTaylorBotApplication(env)
                        .AddDatabaseConnection(env)
                        .AddRedisConnection(env)
                        .AddCommandClient(env)
                        .AddJsonFile(path: "Settings/taypointWill.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: "Settings/dailyPayout.json", optional: false, reloadOnChange: true);

                    appConfig.AddEnvironmentVariables("TaylorBot_");
                })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    var config = hostBuilderContext.Configuration;
                    services
                        .AddHostedService<TaylorBotCommandHostedService>()
                        .AddCommandApplication(config)
                        .AddCommandInfrastructure(config)
                        .AddPostgresConnection(config)
                        .AddRedisConnection(config)
                        .ConfigureRequired<TaypointWillOptions>(config, "TaypointWill")
                        .AddTransient<UserStatusStringMapper>()
                        .AddTransient<ITaypointWillRepository, TaypointWillPostgresRepository>()
                        .AddTransient<IJailRepository, JailPostgresRepository>()
                        .ConfigureRequired<DailyPayoutOptions>(config, "DailyPayout")
                        .AddTransient<IDailyPayoutRepository, DailyPayoutPostgresRepository>()
                        .AddTransient<IMessageOfTheDayRepository, MessageOfTheDayPostgresRepository>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
