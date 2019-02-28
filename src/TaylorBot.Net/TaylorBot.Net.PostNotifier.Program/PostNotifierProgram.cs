using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Environment;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.PostNotifier.Program.Events;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.RedditNotifier.Domain.Options;
using TaylorBot.Net.RedditNotifier.Infrastructure;
using Reddit;
using Microsoft.Extensions.Options;
using TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.PostNotifier.Program
{
    public class PostNotifierProgram
    {
        public static async Task Main()
        {
            using (var services = new PostNotifierProgram().ConfigureServices())
            {
                await new TaylorBotApplication(services).StartAsync();
            }
        }

        private ServiceProvider ConfigureServices()
        {
            var environment = TaylorBotEnvironment.CreateCurrent();

            var config = new ConfigurationBuilder()
                .AddTaylorBotApplicationConfiguration(environment)
                .AddJsonFile(path: $"Settings/redditNotifier.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/redditAuth.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/databaseConnection.{environment}.json", optional: false)
                .Build();

            return new ServiceCollection()
                .AddTaylorBotApplicationServices(config)
                .AddTaylorBotApplicationLogging(config)
                .Configure<RedditNotifierOptions>(config.GetSection("RedditNotifier"))
                .Configure<RedditAuthOptions>(config.GetSection("RedditAuth"))
                .Configure<DatabaseConnectionOptions>(config.GetSection("DatabaseConnection"))
                .AddSingleton<IConfiguration>(config)
                .AddSingleton(provider =>
                {
                    var auth = provider.GetRequiredService<IOptionsMonitor<RedditAuthOptions>>().CurrentValue;
                    return new RedditAPI(appId: auth.AppId, appSecret: auth.AppSecret, refreshToken: auth.RefreshToken);
                })
                .AddTransient<IRedditCheckerRepository, RedditCheckerRepository>()
                .AddTransient<SingletonTaskRunner>()
                .AddTransient<RedditNotiferDomainService>()
                .AddTransient<RedditPostToEmbedMapper>()
                .AddTransient<IReadyHandler, ReadyHandler>()
                .BuildServiceProvider();
        }
    }
}
