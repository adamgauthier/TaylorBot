using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Infrastructure;
using TaylorBot.Net.Commands.Extensions;
using TaylorBot.Net.Commands.Infrastructure;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program.Extensions;

namespace TaylorBot.Net.Commands.Discord.Program
{
    public class DiscordCommandsProgram
    {
        public static async Task Main()
        {
            var environment = TaylorBotEnvironment.CreateCurrent();

            var host = new HostBuilder()
                .UseEnvironment(environment.ToString())
                .ConfigureAppConfiguration((hostBuilderContext, appConfig) =>
                {
                    var env = hostBuilderContext.HostingEnvironment.EnvironmentName;
                    appConfig
                        .AddTaylorBotApplication(environment)
                        .AddDatabaseConnection(environment)
                        .AddRedisConnection(environment)
                        .AddCommandClient(environment)
                        .AddJsonFile(path: $"Settings/taypointWill.{env}.json", optional: false);
                })
                .ConfigureLogging((hostBuilderContext, logging) =>
                {
                    logging.AddTaylorBotApplicationLogging(hostBuilderContext.Configuration);
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
                        .AddTransient<ITaypointWillRepository, TaypointWillPostgresRepository>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
