using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.Domain;
using TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.TypeReaders;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Jail.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.LastFm.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.LastFm.TypeReaders;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.ServerStats.Domain;
using TaylorBot.Net.Commands.Discord.Program.ServerStats.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Discord.Program.TaypointReward.Domain;
using TaylorBot.Net.Commands.Discord.Program.TaypointReward.Infrastructure;
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
                        .AddJsonFile(path: "Settings/dailyPayout.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: "Settings/lastFm.json", optional: false, reloadOnChange: true);

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
                        .AddTransient<ChannelTypeStringMapper>()
                        .AddTransient<ITaypointWillRepository, TaypointWillPostgresRepository>()
                        .AddTransient<IJailRepository, JailPostgresRepository>()
                        .ConfigureRequired<DailyPayoutOptions>(config, "DailyPayout")
                        .AddTransient<IDailyPayoutRepository, DailyPayoutPostgresRepository>()
                        .AddTransient<IMessageOfTheDayRepository, MessageOfTheDayPostgresRepository>()
                        .AddTransient<ICryptoSecureRandom, CryptoSecureRandom>()
                        .AddTransient<IServerStatsRepository, ServerStatsRepositoryPostgresRepository>()
                        .AddTransient<ILastFmUsernameRepository, LastFmUsernamePostgresRepository>()
                        .ConfigureRequired<LastFmOptions>(config, "LastFm")
                        .AddTransient(provider =>
                        {
                            var options = provider.GetRequiredService<IOptionsMonitor<LastFmOptions>>().CurrentValue;
                            return new LastfmClient(
                                apiKey: options.LastFmApiKey, apiSecret: options.LastFmApiSecret
                            );
                        })
                        .AddTransient<ILastFmClient, InflatableLastFmClient>()
                        .AddTransient<LastFmPeriodStringMapper>()
                        .AddTransient<LastFmCollageSize.Factory>()
                        .AddTransient<ITaylorBotTypeReader, LastFmCollageSizeTypeReader>()
                        .AddTransient<ITaylorBotTypeReader, LastFmUsernameTypeReader>()
                        .AddTransient<ITaylorBotTypeReader, LastFmPeriodTypeReader>()
                        .AddTransient<ITaypointRewardRepository, TaypointRewardPostgresRepository>()
                        .AddTransient<IAccessibleRoleRepository, AccessibleRolePostgresRepository>()
                        .AddTransient<ITaylorBotTypeReader, AccessibleGroupNameTypeReader>()
                        .AddTransient<IBotInfoRepository, BotInfoRepositoryPostgresRepository>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
