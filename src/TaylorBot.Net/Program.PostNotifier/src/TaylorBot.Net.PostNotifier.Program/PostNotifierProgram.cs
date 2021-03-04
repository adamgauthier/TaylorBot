using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using DontPanic.TumblrSharp.OAuth;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Reddit;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.InstagramNotifier.Domain;
using TaylorBot.Net.InstagramNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.InstagramNotifier.Domain.Options;
using TaylorBot.Net.InstagramNotifier.Infrastructure;
using TaylorBot.Net.PostNotifier.Program.Events;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.RedditNotifier.Domain.Options;
using TaylorBot.Net.RedditNotifier.Infrastructure;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.TumblrNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.TumblrNotifier.Domain.Options;
using TaylorBot.Net.TumblrNotifier.Infrastructure;
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
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostBuilderContext, appConfig) =>
                {
                    var env = hostBuilderContext.HostingEnvironment;

                    appConfig
                        .AddTaylorBotApplication(env)
                        .AddDatabaseConnection(env)
                        .AddJsonFile(path: "Settings/redditNotifier.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: "Settings/youtubeNotifier.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: "Settings/tumblrNotifier.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: "Settings/instagramNotifier.json", optional: false, reloadOnChange: true);

                    appConfig.AddEnvironmentVariables("TaylorBot_");
                })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    var config = hostBuilderContext.Configuration;
                    services
                        .AddHostedService<TaylorBotHostedService>()
                        .AddTaylorBotApplicationServices(config)
                        .AddPostgresConnection(config)
                        .ConfigureRequired<RedditNotifierOptions>(config, "RedditNotifier")
                        .ConfigureRequired<RedditAuthOptions>(config, "RedditAuth")
                        .AddSingleton(provider =>
                        {
                            var auth = provider.GetRequiredService<IOptionsMonitor<RedditAuthOptions>>().CurrentValue;
                            return new RedditClient(appId: auth.AppId, appSecret: auth.AppSecret, refreshToken: auth.RefreshToken);
                        })
                        .ConfigureRequired<YoutubeNotifierOptions>(config, "YoutubeNotifier")
                        .ConfigureRequired<YoutubeAuthOptions>(config, "YoutubeAuth")
                        .AddSingleton(provider =>
                        {
                            var auth = provider.GetRequiredService<IOptionsMonitor<YoutubeAuthOptions>>().CurrentValue;
                            return new BaseClientService.Initializer() { ApiKey = auth.ApiKey };
                        })
                        .ConfigureRequired<TumblrNotifierOptions>(config, "TumblrNotifier")
                        .ConfigureRequired<TumblrAuthOptions>(config, "TumblrAuth")
                        .AddSingleton(provider =>
                        {
                            var auth = provider.GetRequiredService<IOptionsMonitor<TumblrAuthOptions>>().CurrentValue;
                            return new TumblrClientFactory().Create<TumblrClient>(
                                consumerKey: auth.ConsumerKey,
                                consumerSecret: auth.ConsumerSecret,
                                new Token(key: auth.Token, secret: auth.TokenSecret)
                            );
                        })
                        .ConfigureRequired<InstagramNotifierOptions>(config, "InstagramNotifier")
                        .AddTransient<IShardReadyHandler, ReadyHandler>()
                        .AddTransient<SingletonTaskRunner>()
                        .AddTransient<IRedditCheckerRepository, RedditCheckerRepository>()
                        .AddTransient<RedditNotifierService>()
                        .AddTransient<RedditPostToEmbedMapper>()
                        .AddTransient<YouTubeService>()
                        .AddTransient<IYoutubeCheckerRepository, YoutubeCheckerRepository>()
                        .AddTransient<YoutubeNotifierService>()
                        .AddTransient<YoutubePostToEmbedMapper>()
                        .AddTransient<ITumblrCheckerRepository, TumblrCheckerRepository>()
                        .AddTransient<TumblrNotifierService>()
                        .AddTransient<TumblrPostToEmbedMapper>()
                        .AddTransient<IInstagramCheckerRepository, InstagramCheckerPostgresRepository>()
                        .AddTransient<IInstagramClient, InstagramRestClient>()
                        .AddTransient<InstagramNotifierService>()
                        .AddTransient<InstagramPostToEmbedMapper>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
