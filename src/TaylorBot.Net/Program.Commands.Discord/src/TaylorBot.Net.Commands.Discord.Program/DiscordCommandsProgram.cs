using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Extensions;
using TaylorBot.Net.Commands.Infrastructure;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program.Extensions;


var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((hostBuilderContext, appConfig) =>
    {
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
            .AddTransient<IBotInfoRepository, BotInfoRepositoryPostgresRepository>()
            .AddTransient<IUsernameHistoryRepository, UsernameHistoryPostgresRepository>()
            .AddTransient<IPlusUserRepository, PlusUserPostgresRepository>()
            .ConfigureRequired<ImageOptions>(config, "Image")
            .AddSingleton(provider =>
            {
                var auth = provider.GetRequiredService<IOptionsMonitor<ImageOptions>>().CurrentValue;
                return new BaseClientService.Initializer { ApiKey = auth.GoogleApiKey };
            })
            .AddTransient<CustomSearchAPIService>()
            .AddTransient<IImageSearchClient, GoogleCustomSearchClient>()
            .AddTransient<IDeletedLogChannelRepository, DeletedLogChannelPostgresRepository>()
            .AddTransient<IMemberLogChannelRepository, MemberLogChannelPostgresRepository>()
            .AddTransient<IModLogChannelRepository, ModLogChannelPostgresRepository>()
            .AddSlashCommand<ModLogSetSlashCommand, ModLogSetSlashCommand.Options>()
            .AddSlashCommand<ModLogStopSlashCommand, NoOptions>()
            .AddSlashCommand<ModLogViewSlashCommand, NoOptions>()
            .AddSlashCommand<AvatarSlashCommand, AvatarSlashCommand.Options>();
    })
    .Build();

await host.RunAsync();
