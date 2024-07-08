using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using TaylorBot.Net.Commands;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Channel.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Channel.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Commands.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Poll.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Signature.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Extensions;
using TaylorBot.Net.Commands.Infrastructure;
using TaylorBot.Net.Commands.Infrastructure.Options;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Random;

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
            .AddJsonFile(path: "Settings/modMail.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: "Settings/heist.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: $"Settings/heist.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            ;

        appConfig.AddEnvironmentVariables("TaylorBot_");
    })
    .ConfigureServices((hostBuilderContext, services) =>
    {
        var config = hostBuilderContext.Configuration;
        services
            .AddHostedService<TaylorBotCommandHostedService>()
            .AddCommandApplication(config, hostBuilderContext.HostingEnvironment)
            .AddCommandInfrastructure(config)
            .AddPostgresConnection(config)
            .AddRedisConnection(config)
            .ConfigureRequired<TaypointWillOptions>(config, "TaypointWill")
            .AddTransient<ChannelTypeStringMapper>()
            .AddTransient<ITaypointWillRepository, TaypointWillPostgresRepository>()
            .AddTransient<IJailRepository, JailPostgresRepository>()
            .ConfigureRequired<DailyPayoutOptions>(config, "DailyPayout")
            .AddTransient<IDailyPayoutRepository, DailyPayoutPostgresRepository>()
            .AddTransient<IMessageOfTheDayRepository, MessageOfTheDayPostgresRepository>()
            .AddTransient<ICryptoSecureRandom, CryptoSecureRandom>()
            .AddTransient<IPseudoRandom, PseudoRandom>()
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
            .AddSlashCommand<PlusShowSlashCommand>()
            .AddSlashCommand<PlusAddSlashCommand>()
            .AddSlashCommand<PlusRemoveSlashCommand>()
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
            .AddTransient<ModMailChannelLogger>()
            .AddSlashCommand<ModLogSetSlashCommand>()
            .AddSlashCommand<ModLogStopSlashCommand>()
            .AddSlashCommand<ModLogShowSlashCommand>()
            .ConfigureRequired<ModMailOptions>(config, "ModMail")
            .AddSlashCommand<ModMailMessageUserSlashCommand>()
            .AddSlashCommand<ModMailMessageModsSlashCommand>()
            .AddTransient<IModMailBlockedUsersRepository, ModMailBlockedUsersPostgresRepository>()
            .AddTransient<IModMailLogChannelRepository, ModMailLogChannelPostgresRepository>()
            .AddSlashCommand<ModMailBlockSlashCommand>()
            .AddSlashCommand<ModMailUnblockSlashCommand>()
            .AddSlashCommand<ModMailLogSetSlashCommand>()
            .AddSlashCommand<ModMailLogStopSlashCommand>()
            .AddSlashCommand<ModMailLogShowSlashCommand>()
            .AddSlashCommand<AvatarSlashCommand>()
            .AddOptionParser<OptionalAvatarTypeParser>()
            .AddSlashCommand<KickSlashCommand>()
            .AddSlashCommand<ChooseSlashCommand>()
            .AddSlashCommand<DiceSlashCommand>()
            .AddTransient<IReminderRepository, ReminderPostgresRepository>()
            .AddSlashCommand<RemindAddSlashCommand>()
            .AddSlashCommand<RemindManageSlashCommand>()
            .AddTransient<YouTubeService>()
            .AddTransient<IYouTubeClient, YouTubeClient>()
            .AddSlashCommand<YouTubeSlashCommand>()
            .AddSlashCommand<OwnerIgnoreSlashCommand>()
            .AddSlashCommand<OwnerRewardSlashCommand>()
            .AddSlashCommand<OwnerAddFeedbackUsersSlashCommand>()
            .AddSlashCommand<ImageSlashCommand>()
            .AddSlashCommand<DailyRebuySlashCommand>()
            .AddTransient<DailyClaimCommand>()
            .AddSlashCommand<DailyClaimSlashCommand>()
            .AddSlashCommand<DailyStreakSlashCommand>()
            .AddSlashCommand<DailyLeaderboardSlashCommand>()
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
            .AddSlashCommand<LastFmCollageSlashCommand>()
            .AddOptionParser<OptionalLastFmPeriodParser>()
            .AddTransient<LastFmTracksCommand>()
            .AddSlashCommand<LastFmTracksSlashCommand>()
            .AddTransient<LastFmAlbumsCommand>()
            .AddSlashCommand<LastFmAlbumsSlashCommand>()
            .AddTransient<LastFmArtistsCommand>()
            .AddSlashCommand<LastFmArtistsSlashCommand>()
            .AddTransient<IHoroscopeClient, GaneshaSpeaksHoroscopeClient>()
            .AddTransient<IZodiacSignRepository, ZodiacSignPostgresRepository>()
            .AddTransient<IBirthdayRepository, BirthdayPostgresRepository>()
            .AddSlashCommand<BirthdayShowSlashCommand>()
            .AddSlashCommand<BirthdayClearSlashCommand>()
            .AddSlashCommand<BirthdaySetSlashCommand>()
            .AddSlashCommand<BirthdayCalendarSlashCommand>()
            .AddSlashCommand<BirthdayHoroscopeSlashCommand>()
            .AddTransient<AgeCalculator>()
            .AddSlashCommand<BirthdayAgeSlashCommand>()
            .AddTransient<IBirthdayRoleConfigRepository, BirthdayRoleConfigPostgresRepository>()
            .AddSlashCommand<BirthdayRoleSlashCommand>()
            .AddTransient<IUrbanDictionaryClient, UrbanDictionaryClient>()
            .AddTransient<UrbanDictionaryCommand>()
            .AddSlashCommand<UrbanDictionarySlashCommand>()
            .ConfigureRequired<WolframAlphaOptions>(config, "WolframAlpha")
            .AddTransient<IWolframAlphaClient, WolframAlphaClient>()
            .AddSlashCommand<WolframAlphaSlashCommand>()
            .AddSlashCommand<ImgurSlashCommand>()
            .ConfigureRequired<ImgurOptions>(config, "Imgur")
            .AddSingleton<IValidateOptions<ImgurOptions>, ImgurOptionsValidator>()
            .AddTransient<ITaypointBalanceRepository, TaypointBalancePostgresRepository>()
            .AddOptionParser<TaypointAmountParser>()
            .AddTransient<MemberNotInGuildUpdater>()
            .AddTransient<TaypointGuildCacheUpdater>()
            .AddSlashCommand<TaypointsBalanceSlashCommand>()
            .AddSlashCommand<TaypointsLeaderboardSlashCommand>()
            .AddSlashCommand<PollSlashCommand>()
            .AddSlashCommand<ModSpamAddSlashCommand>()
            .AddSlashCommand<ModSpamRemoveSlashCommand>()
            .AddTransient<ITaypointTransferRepository, TaypointTransferPostgresRepository>()
            .AddSlashCommand<TaypointsGiftSlashCommand>()
            .AddTransient<IGuildNamesRepository, GuildNamesPostgresRepository>()
            .AddSlashCommand<ServerNamesSlashCommand>()
            .AddSlashCommand<ServerPopulationSlashCommand>()
            .AddTransient<IChannelMessageCountRepository, ChannelMessageCountPostgresRepository>()
            .AddSlashCommand<ChannelMessagesSlashCommand>()
            .AddTransient<ILocationClient, GooglePlacesClient>()
            .AddTransient<ILocationRepository, LocationPostgresRepository>()
            .AddTransient<LocationFetcherDomainService>()
            .AddTransient<LocationShowCommand>()
            .AddSlashCommand<LocationShowSlashCommand>()
            .AddSlashCommand<LocationTimeSlashCommand>()
            .AddSlashCommand<LocationSetSlashCommand>()
            .AddSlashCommand<LocationClearSlashCommand>()
            .ConfigureRequired<WeatherOptions>(config, "Weather")
            .AddTransient<IWeatherClient, PirateWeatherClient>()
            .AddTransient<WeatherCommand>()
            .AddSlashCommand<WeatherSlashCommand>()
            .AddTransient<TextAttributePostgresRepository>()
            .AddTransient<IGenderRepository, GenderPostgresRepository>()
            .AddSlashCommand<GenderShowSlashCommand>()
            .AddSlashCommand<GenderSetSlashCommand>()
            .AddSlashCommand<GenderClearSlashCommand>()
            .AddTransient<IServerJoinedRepository, ServerJoinedPostgresRepository>()
            .AddSlashCommand<ServerJoinedSlashCommand>()
            .AddSlashCommand<ServerTimelineSlashCommand>()
            .AddTransient<IServerActivityRepository, ServerActivityPostgresRepository>()
            .AddSlashCommand<ServerMessagesSlashCommand>()
            .AddSlashCommand<ServerMinutesSlashCommand>()
            .AddSlashCommand<ServerLeaderboardSlashCommand>()
            .AddTransient<IFavoriteSongsRepository, FavoriteSongsPostgresRepository>()
            .AddSlashCommand<FavoriteSongsShowSlashCommand>()
            .AddSlashCommand<FavoriteSongsSetSlashCommand>()
            .AddSlashCommand<FavoriteSongsClearSlashCommand>()
            .AddTransient<IBaeRepository, BaePostgresRepository>()
            .AddSlashCommand<FavoriteBaeShowSlashCommand>()
            .AddSlashCommand<FavoriteBaeSetSlashCommand>()
            .AddSlashCommand<FavoriteBaeClearSlashCommand>()
            .AddTransient<IObsessionRepository, ObsessionPostgresRepository>()
            .AddSlashCommand<FavoriteObsessionShowSlashCommand>()
            .AddSlashCommand<FavoriteObsessionSetSlashCommand>()
            .AddSlashCommand<FavoriteObsessionClearSlashCommand>()
            .ConfigureRequired<SignatureOptions>(config, "Signature")
            .AddSlashCommand<SignatureSlashCommand>()
            .AddSlashCommand<OwnerDownloadAvatarsSlashCommand>()
            .ConfigureRequired<HeistOptions>(config, "Heist")
            .AddSingleton<IValidateOptions<HeistOptions>, HeistOptionsValidator>()
            .AddTransient<IHeistStatsRepository, HeistStatsPostgresRepository>()
            .AddSingleton<HeistInMemoryRepository>()
            .AddTransient<HeistRedisRepository>()
            .AddTransient(provider =>
            {
                var options = provider.GetRequiredService<IOptionsMonitor<CommandClientOptions>>().CurrentValue;
                return options.UseRedisCache ?
                    provider.GetRequiredService<HeistRedisRepository>() :
                    (IHeistRepository)provider.GetRequiredService<HeistInMemoryRepository>();
            })
            .AddSlashCommand<HeistPlaySlashCommand>()
            .AddSlashCommand<HeistProfileSlashCommand>()
            .AddSlashCommand<HeistLeaderboardSlashCommand>()
            .AddTransient<IRpsStatsRepository, RpsStatsPostgresRepository>()
            .AddSlashCommand<RpsProfileSlashCommand>()
            .AddOptionParser<OptionalRpsShapeParser>()
            .AddSlashCommand<RpsPlaySlashCommand>()
            .AddSlashCommand<RpsLeaderboardSlashCommand>()
            .AddSlashCommand<OwnerRewardYearbookActiveMembersSlashCommand>()
            .AddTransient<IRiskStatsRepository, RiskStatsPostgresRepository>()
            .AddOptionParser<OptionalRiskLevelParser>()
            .AddSlashCommand<RiskPlaySlashCommand>()
            .AddSlashCommand<RiskProfileSlashCommand>()
            .AddSlashCommand<RiskLeaderboardSlashCommand>()
            .AddTransient<IRollStatsRepository, RollStatsPostgresRepository>()
            .AddSlashCommand<RollPlaySlashCommand>()
            .AddSlashCommand<RollProfileSlashCommand>()
            .AddSlashCommand<RollLeaderboardSlashCommand>()
            .AddSlashCommand<UsernamesShowSlashCommand>()
            .AddSlashCommand<UsernamesVisibilitySlashCommand>()
            ;

        services.AddHttpClient<ImgurClient, ImgurHttpClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<ImgurOptions>>().CurrentValue;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", options.ClientId);
        });
    })
    .Build();

await host.RunAsync();
