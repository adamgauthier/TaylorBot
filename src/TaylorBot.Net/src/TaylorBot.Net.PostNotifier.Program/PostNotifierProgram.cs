using Google.Apis.Services;
using Google.Apis.YouTube.v3;
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
using TaylorBot.Net.YoutubeNotifier.Domain;
using TaylorBot.Net.YoutubeNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.YoutubeNotifier.Domain.Options;
using TaylorBot.Net.YoutubeNotifier.Infrastructure;

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
                .AddJsonFile(path: $"Settings/youtubeNotifier.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/youtubeAuth.{environment}.json", optional: false)
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
                .ConfigureRequired<YoutubeNotifierOptions>(config, "YoutubeNotifier")
                .ConfigureRequired<YoutubeAuthOptions>(config, "YoutubeAuth")
                .AddSingleton(provider =>
                {
                    var auth = provider.GetRequiredService<IOptionsMonitor<YoutubeAuthOptions>>().CurrentValue;
                    return new BaseClientService.Initializer() { ApiKey = auth.ApiKey };
                })
                .AddTransient<IShardReadyHandler, ReadyHandler>()
                .AddTransient<SingletonTaskRunner>()
                .AddTransient<IRedditCheckerRepository, RedditCheckerRepository>()
                .AddTransient<RedditNotifierService>()
                .AddTransient<RedditPostToEmbedMapper>()
                .AddTransient<YouTubeService>()
                .AddTransient<IYoutubeCheckerRepository, YoutubeCheckerRepository>()
                .AddTransient<YoutubeNotifierService>()
                .AddTransient<YoutubePostToEmbedMapper>()
                .BuildServiceProvider();
        }
    }
}
