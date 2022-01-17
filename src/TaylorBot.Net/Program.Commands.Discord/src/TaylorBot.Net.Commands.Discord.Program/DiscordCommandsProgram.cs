using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using TaylorBot.Net.Commands;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Commands.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;
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
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Extensions;
using TaylorBot.Net.Commands.Infrastructure;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program.Extensions;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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
            .AddJsonFile(path: "Settings/lastFm.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: "Settings/modMail.json", optional: false, reloadOnChange: true);

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
            .ConfigureRequired<GoogleOptions>(config, "Google")
            .AddSingleton(provider =>
            {
                var auth = provider.GetRequiredService<IOptionsMonitor<GoogleOptions>>().CurrentValue;
                return new BaseClientService.Initializer { ApiKey = auth.GoogleApiKey };
            })
            .AddTransient<CustomSearchAPIService>()
            .ConfigureRequired<ImageOptions>(config, "Image")
            .AddTransient<IImageSearchClient, GoogleCustomSearchClient>()
            .AddTransient<IDeletedLogChannelRepository, DeletedLogChannelPostgresRepository>()
            .AddTransient<IMemberLogChannelRepository, MemberLogChannelPostgresRepository>()
            .AddTransient<IModLogChannelRepository, ModLogChannelPostgresRepository>()
            .AddTransient<IModChannelLogger, ModChannelLogger>()
            .AddSlashCommand<ModLogSetSlashCommand>()
            .AddSlashCommand<ModLogStopSlashCommand>()
            .AddSlashCommand<ModLogShowSlashCommand>()
            .ConfigureRequired<ModMailOptions>(config, "ModMail")
            .AddSlashCommand<ModMailMessageUserSlashCommand>()
            .AddSlashCommand<ModMailMessageModsSlashCommand>()
            .AddTransient<IModMailBlockedUsersRepository, ModMailBlockedUsersPostgresRepository>()
            .AddSlashCommand<ModMailBlockSlashCommand>()
            .AddSlashCommand<ModMailUnblockSlashCommand>()
            .AddSlashCommand<AvatarSlashCommand>()
            .AddSlashCommand<KickSlashCommand>()
            .AddSlashCommand<ChooseSlashCommand>()
            .AddTransient<IReminderRepository, ReminderPostgresRepository>()
            .AddSlashCommand<RemindAddSlashCommand>()
            .AddSlashCommand<RemindManageSlashCommand>()
            .AddTransient<YouTubeService>()
            .AddTransient<IYouTubeClient, YouTubeClient>()
            .AddSlashCommand<YouTubeSlashCommand>()
            .AddSlashCommand<OwnerIgnoreSlashCommand>()
            .AddSlashCommand<OwnerRewardSlashCommand>()
            .AddSlashCommand<ImageSlashCommand>()
            .AddSlashCommand<DailyRebuySlashCommand>()
            .AddTransient<DailyClaimCommand>()
            .AddSlashCommand<DailyClaimSlashCommand>()
            .AddSlashCommand<DailyStreakSlashCommand>()
            .AddTransient<IEditedLogChannelRepository, EditedLogChannelPostgresRepository>()
            .AddSlashCommand<MonitorEditedSetSlashCommand>()
            .AddSlashCommand<MonitorEditedShowSlashCommand>()
            .AddSlashCommand<MonitorEditedStopSlashCommand>()
            .AddSlashCommand<MonitorDeletedSetSlashCommand>()
            .AddSlashCommand<MonitorDeletedShowSlashCommand>()
            .AddSlashCommand<MonitorDeletedStopSlashCommand>()
            .AddSlashCommand<MonitorMembersSetSlashCommand>()
            .AddSlashCommand<MonitorMembersShowSlashCommand>()
            .AddSlashCommand<MonitorMembersStopSlashCommand>()
            .AddSlashCommand<CommandServerDisableSlashCommand>()
            .AddSlashCommand<CommandServerEnableSlashCommand>()
            .AddSlashCommand<CommandChannelDisableSlashCommand>()
            .AddSlashCommand<CommandChannelEnableSlashCommand>()
            .AddTransient<LastFmEmbedFactory>()
            .AddTransient<LastFmCurrentCommand>()
            .AddSlashCommand<LastFmCurrentSlashCommand>()
            .AddTransient<LastFmSetCommand>()
            .AddSlashCommand<LastFmSetSlashCommand>()
            .AddOptionParser<LastFmUsernameParser>()
            .AddTransient<LastFmClearCommand>()
            .AddSlashCommand<LastFmClearSlashCommand>()
            .AddTransient<LastFmCollageCommand>()
            .AddSlashCommand<LastFmCollageSlashCommand>()
            .AddOptionParser<OptionalLastFmPeriodParser>()
            .AddTransient<LastFmTracksCommand>()
            .AddSlashCommand<LastFmTracksSlashCommand>()
            .AddTransient<LastFmAlbumsCommand>()
            .AddSlashCommand<LastFmAlbumsSlashCommand>()
            .AddTransient<LastFmArtistsCommand>()
            .AddSlashCommand<LastFmArtistsSlashCommand>();
    })
    .Build();

await host.RunAsync();
