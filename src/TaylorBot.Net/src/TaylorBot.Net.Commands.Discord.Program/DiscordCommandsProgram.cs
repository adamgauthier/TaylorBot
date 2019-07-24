using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Extensions;
using TaylorBot.Net.Commands.Infrastructure;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Infrastructure.Options;
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
                        .AddTaylorBotApplicationConfiguration(environment)
                        .AddDatabaseConnectionConfiguration(environment)
                        .AddJsonFile(path: $"Settings/redisCommandsConnection.{env}.json", optional: false)
                        .AddJsonFile(path: $"Settings/commandClient.{env}.json", optional: false);
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
                        .AddTaylorBotCommandServices(config)
                        .ConfigureDatabaseConnection(config)
                        .ConfigureRequired<RedisConnectionOptions>(config, "RedisCommandsConnection")
                        .ConfigureRequired<CommandClientOptions>(config, "CommandClient")
                        .AddSingleton(provider =>
                        {
                            var options = provider.GetRequiredService<IOptionsMonitor<RedisConnectionOptions>>().CurrentValue;
                            return ConnectionMultiplexer.Connect($"{options.Host}:{options.Port},password={options.Password}");
                        })
                        .AddTransient<CommandPrefixPostgresRepository>()
                        .AddTransient<CommandPrefixRedisCacheRepository>()
                        .AddTransient(provider =>
                        {
                            var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                            return options.UseRedisCache ?
                                provider.GetRequiredService<CommandPrefixRedisCacheRepository>() :
                                (ICommandPrefixRepository)provider.GetRequiredService<CommandPrefixPostgresRepository>();
                        });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
