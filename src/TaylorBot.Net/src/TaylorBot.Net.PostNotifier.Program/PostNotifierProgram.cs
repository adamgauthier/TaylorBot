using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reddit;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.PostNotifier.Program.Events;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.RedditNotifier.Domain.Options;
using TaylorBot.Net.RedditNotifier.Infrastructure;

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
                .AddDatabaseConnectionConfiguration(environment)
                .AddJsonFile(path: $"Settings/redditNotifier.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/redditAuth.{environment}.json", optional: false)
                .Build();

            return new ServiceCollection()
                .AddTaylorBotApplicationServices(config)
                .AddTaylorBotApplicationLogging(config)
                .ConfigureDatabaseConnection(config)
                .ConfigureRequired<RedditNotifierOptions>(config, "RedditNotifier")
                .ConfigureRequired<RedditAuthOptions>(config, "RedditAuth")
                .AddSingleton(provider =>
                {
                    var auth = provider.GetRequiredService<IOptionsMonitor<RedditAuthOptions>>().CurrentValue;
                    return new RedditAPI(appId: auth.AppId, appSecret: auth.AppSecret, refreshToken: auth.RefreshToken);
                })
                .AddTransient<IShardReadyHandler, ReadyHandler>()
                .AddTransient<SingletonTaskRunner>()
                .AddTransient<IRedditCheckerRepository, RedditCheckerRepository>()
                .AddTransient<RedditNotifierService>()
                .AddTransient<RedditPostToEmbedMapper>()
                .BuildServiceProvider();
        }
    }
}
