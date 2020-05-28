using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MessagesTracker.Infrastructure;
using TaylorBot.Net.MinutesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Domain.Options;
using TaylorBot.Net.MinutesTracker.Infrastructure;
using TaylorBot.Net.StatsTracker.Program.Events;

namespace TaylorBot.Net.StatsTracker.Program
{
    public class StatsTrackerProgram
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
                        .AddJsonFile(path: "Settings/minutesTracker.json", optional: false);

                    appConfig.AddEnvironmentVariables("TaylorBot_");
                })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    var config = hostBuilderContext.Configuration;
                    services
                        .AddHostedService<TaylorBotHostedService>()
                        .AddTaylorBotApplicationServices(config)
                        .AddPostgresConnection(config)
                        .ConfigureRequired<MinutesTrackerOptions>(config, "MinutesTracker")
                        .AddTransient<IShardReadyHandler, ReadyHandler>()
                        .AddTransient<IUserMessageReceivedHandler, UserMessageReceivedHandler>()
                        .AddTransient<SingletonTaskRunner>()
                        .AddTransient<IMinuteRepository, MinutesRepository>()
                        .AddTransient<MinutesTrackerDomainService>()
                        .AddTransient<IMessageRepository, MessagesRepository>()
                        .AddTransient<WordCounter>()
                        .AddTransient<MessagesTrackerDomainService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
