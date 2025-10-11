using Azure.Identity;
using Azure.Storage.Blobs;
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
using TaylorBot.Net.Commands.Discord.Program.Modules.Events;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Coupons;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Help.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Help.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Help.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Infrastructure;
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
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;
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
using TaylorBot.Net.Core.Program;
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
            .AddJsonFile(path: "Settings/taypointWill.json", optional: false)
            .AddJsonFile(path: "Settings/dailyPayout.json", optional: false)
            .AddJsonFile(path: "Settings/lastFm.json", optional: false)
            .AddJsonFile(path: "Settings/modMail.json", optional: false)
            .AddJsonFile(path: "Settings/heist.json", optional: false)
            .AddJsonFile(path: $"Settings/heist.{env.EnvironmentName}.json", optional: true)
            ;

        appConfig.AddEnvironmentVariables("TaylorBot_");
    })
    .ConfigureServices(DiscordCommandsProgram.ConfigureServices)
    .Build();

await host.RunAsync();

public static class DiscordCommandsProgram
{
    public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        var config = hostBuilderContext.Configuration;
        services
            .AddHttpClient()
            .AddMemoryCache()
            .AddTransient<TaylorBotHostedService>()
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
            .AddTransient<ICommandsHelpRepository, CommandsHelpPostgresRepository>()
            .AddTransient<CommandCategoryService>()
            .AddSlashCommand<HelpSlashCommand>()
            .AddStringSelectHandler<HelpCategoryHandler>()
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
            .AddButtonHandler<ModLogSetConfirmButtonHandler>()
            .AddButtonHandler<ModLogStopButtonHandler>()
            .AddSlashCommand<ModLogShowSlashCommand>()
            .ConfigureRequired<ModMailOptions>(config, "ModMail")
            .AddSlashCommand<ModMailMessageUserSlashCommand>()
            .AddSlashCommand<ModMailMessageModsSlashCommand>()
            .AddButtonHandler<ModMailMessageModsConfirmButtonHandler>()
            .AddButtonHandler<ModMailUserMessageReplyButtonHandler>()
            .AddModalHandler<ModMailMessageModsModalHandler>()
            .AddModalHandler<ModMailUserMessageReplyModalHandler>()
            .AddButtonHandler<ModMailReplyConfirmButtonHandler>()
            .AddTransient<IModMailBlockedUsersRepository, ModMailBlockedUsersPostgresRepository>()
            .AddTransient<IModMailLogChannelRepository, ModMailLogChannelPostgresRepository>()
            .AddSlashCommand<ModMailBlockSlashCommand>()
            .AddSlashCommand<ModMailUnblockSlashCommand>()
            .AddSlashCommand<ModMailConfigSlashCommand>()
            .AddChannelSelectHandler<ModMailConfigSetChannelHandler>()
            .AddButtonHandler<ModMailConfigConfirmHandler>()
            .AddButtonHandler<ModMailConfigStopHandler>()
            .AddSlashCommand<InspectUserSlashCommand>()
            .AddSlashCommand<InspectChannelSlashCommand>()
            .AddSlashCommand<InspectRoleSlashCommand>()
            .AddSlashCommand<InspectServerSlashCommand>()
            .AddSlashCommand<AvatarSlashCommand>()
            .AddOptionParser<OptionalAvatarTypeParser, AvatarType?>()
            .AddSlashCommand<KickSlashCommand>()
            .AddButtonHandler<KickConfirmButtonHandler>()
            .AddSlashCommand<ChooseSlashCommand>()
            .AddSlashCommand<DiceSlashCommand>()
            .AddTransient<IReminderRepository, ReminderPostgresRepository>()
            .AddSlashCommand<RemindAddSlashCommand>()
            .AddSlashCommand<RemindManageSlashCommand>()
            .AddButtonHandler<RemindManageClearButtonHandler>()
            .AddButtonHandler<RemindManageClearAllButtonHandler>()
            .AddTransient<YouTubeService>()
            .AddTransient<IYouTubeClient, YouTubeClient>()
            .AddSlashCommand<YouTubeSlashCommand>()
            .AddSlashCommand<OwnerIgnoreSlashCommand>()
            .AddSlashCommand<OwnerRewardSlashCommand>()
            .AddSlashCommand<OwnerDiagnosticSlashCommand>()
            .AddSlashCommand<OwnerAddFeedbackUsersSlashCommand>()
            .AddSlashCommand<ImageSlashCommand>()
            .AddSlashCommand<DailyRebuySlashCommand>()
            .AddButtonHandler<DailyRebuyConfirmButtonHandler>()
            .AddSlashCommand<DailyClaimSlashCommand>()
            .AddSlashCommand<DailyStreakSlashCommand>()
            .AddSlashCommand<DailyLeaderboardSlashCommand>()
            .AddTransient<IEditedLogChannelRepository, EditedLogChannelPostgresRepository>()
            .AddSlashCommand<MonitorEditedSetSlashCommand>()
            .AddButtonHandler<MonitorEditedSetConfirmButtonHandler>()
            .AddButtonHandler<MonitorEditedStopButtonHandler>()
            .AddSlashCommand<MonitorEditedShowSlashCommand>()
            .AddSlashCommand<MonitorDeletedSetSlashCommand>()
            .AddButtonHandler<MonitorDeletedSetConfirmButtonHandler>()
            .AddButtonHandler<MonitorDeletedStopButtonHandler>()
            .AddSlashCommand<MonitorDeletedShowSlashCommand>()
            .AddSlashCommand<MonitorMembersSetSlashCommand>()
            .AddButtonHandler<MonitorMembersSetConfirmButtonHandler>()
            .AddButtonHandler<MonitorMembersStopButtonHandler>()
            .AddSlashCommand<MonitorMembersShowSlashCommand>()
            .AddSlashCommand<CommandServerDisableSlashCommand>()
            .AddSlashCommand<CommandServerEnableSlashCommand>()
            .AddSlashCommand<CommandChannelDisableSlashCommand>()
            .AddSlashCommand<CommandChannelEnableSlashCommand>()
            .AddSlashCommand<CommandPrefixSlashCommand>()
            .AddButtonHandler<CommandPrefixToggleHandler>()
            .AddTransient<LastFmEmbedFactory>()
            .AddSlashCommand<LastFmCurrentSlashCommand>()
            .AddSlashCommand<LastFmSetSlashCommand>()
            .AddOptionParser<LastFmUsernameParser, LastFmUsername>()
            .AddSlashCommand<LastFmClearSlashCommand>()
            .AddSlashCommand<LastFmCollageSlashCommand>()
            .AddOptionParser<OptionalLastFmPeriodParser, LastFmPeriod?>()
            .AddSlashCommand<LastFmTracksSlashCommand>()
            .AddSlashCommand<LastFmAlbumsSlashCommand>()
            .AddSlashCommand<LastFmArtistsSlashCommand>()
            .AddTransient<IZodiacSignRepository, ZodiacSignPostgresRepository>()
            .AddTransient<IBirthdayRepository, BirthdayPostgresRepository>()
            .AddSlashCommand<BirthdayShowSlashCommand>()
            .AddSlashCommand<BirthdayClearSlashCommand>()
            .AddSlashCommand<BirthdaySetSlashCommand>()
            .AddButtonHandler<BirthdaySetConfirmButtonHandler>()
            .AddSlashCommand<BirthdayCalendarSlashCommand>()
            .AddSlashCommand<BirthdayHoroscopeSlashCommand>()
            .AddTransient<AgeCalculator>()
            .AddSlashCommand<BirthdayAgeSlashCommand>()
            .AddTransient<IBirthdayRoleConfigRepository, BirthdayRoleConfigPostgresRepository>()
            .AddSlashCommand<BirthdayRoleSlashCommand>()
            .AddButtonHandler<BirthdayRoleCreateButtonHandler>()
            .AddButtonHandler<BirthdayRoleRemoveButtonHandler>()
            .AddSlashCommand<UrbanDictionarySlashCommand>()
            .ConfigureRequired<WolframAlphaOptions>(config, "WolframAlpha")
            .AddSlashCommand<WolframAlphaSlashCommand>()
            .AddSlashCommand<ImgurSlashCommand>()
            .ConfigureRequired<ImgurOptions>(config, "Imgur")
            .AddSingleton<IValidateOptions<ImgurOptions>, ImgurOptionsValidator>()
            .AddTransient<ITaypointBalanceRepository, TaypointBalancePostgresRepository>()
            .AddOptionParser<TaypointAmountParser, ITaypointAmount>()
            .AddTransient<MemberNotInGuildUpdater>()
            .AddTransient<TaypointGuildCacheUpdater>()
            .AddSlashCommand<TaypointsBalanceSlashCommand>()
            .AddSlashCommand<TaypointsLeaderboardSlashCommand>()
            .AddSlashCommand<TaypointsSuccessionSlashCommand>()
            .AddButtonHandler<TaypointsSuccessionClaimTaypointsHandler>()
            .AddButtonHandler<TaypointsSuccessionClaimSkipHandler>()
            .AddButtonHandler<TaypointsSuccessionClearSuccessorHandler>()
            .AddUserSelectHandler<TaypointsSuccessionChangeSuccessorHandler>()
            .AddSlashCommand<ModSpamAddSlashCommand>()
            .AddSlashCommand<ModSpamRemoveSlashCommand>()
            .AddTransient<ITaypointTransferRepository, TaypointTransferPostgresRepository>()
            .AddSlashCommand<TaypointsGiftSlashCommand>()
            .AddButtonHandler<TaypointsGiftConfirmButtonHandler>()
            .AddTransient<IGuildNamesRepository, GuildNamesPostgresRepository>()
            .AddSlashCommand<ServerNamesSlashCommand>()
            .AddSlashCommand<ServerPopulationSlashCommand>()
            .AddTransient<IChannelMessageCountRepository, ChannelMessageCountPostgresRepository>()
            .AddSlashCommand<ChannelMessagesSlashCommand>()
            .AddTransient<ILocationRepository, LocationPostgresRepository>()
            .AddTransient<ILocationClient, GooglePlacesNewClient>()
            .AddTransient<LocationFetcherDomainService>()
            .AddTransient<LocationShowCommand>()
            .AddSlashCommand<LocationShowSlashCommand>()
            .AddSlashCommand<LocationTimeSlashCommand>()
            .AddSlashCommand<LocationSetSlashCommand>()
            .AddSlashCommand<LocationClearSlashCommand>()
            .ConfigureRequired<WeatherOptions>(config, "Weather")
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
            .AddButtonHandler<FavoriteSongsSetConfirmButtonHandler>()
            .AddSlashCommand<FavoriteSongsClearSlashCommand>()
            .AddTransient<IBaeRepository, BaePostgresRepository>()
            .AddSlashCommand<FavoriteBaeShowSlashCommand>()
            .AddSlashCommand<FavoriteBaeSetSlashCommand>()
            .AddButtonHandler<FavoriteBaeSetConfirmButtonHandler>()
            .AddSlashCommand<FavoriteBaeClearSlashCommand>()
            .AddTransient<IObsessionRepository, ObsessionPostgresRepository>()
            .AddSlashCommand<FavoriteObsessionShowSlashCommand>()
            .AddSlashCommand<FavoriteObsessionSetSlashCommand>()
            .AddButtonHandler<FavoriteObsessionSetConfirmButtonHandler>()
            .AddSlashCommand<FavoriteObsessionClearSlashCommand>()
            .ConfigureRequired<SignatureOptions>(config, "Signature")
            .AddKeyedSingleton<BlobServiceClient>("SignatureAccount", (provider, key) =>
            {
                var options = provider.GetRequiredService<IOptionsMonitor<SignatureOptions>>().CurrentValue;
                var accountUri = options.StorageAccountUri;

                var env = provider.GetRequiredService<IHostEnvironment>();
                return env.IsDevelopment()
                    ? new(accountUri, new DefaultAzureCredential())
                    : new(accountUri, new ManagedIdentityCredential());
            })
            .AddKeyedSingleton<Lazy<BlobContainerClient>>("SignatureContainer", (provider, key) =>
            {
                return new(() => provider.GetRequiredKeyedService<BlobServiceClient>("SignatureAccount").GetBlobContainerClient("signatures2024"));
            })
            .AddSlashCommand<SignatureSlashCommand>()
            .AddButtonHandler<SignatureConfirmButtonHandler>()
            .AddKeyedSingleton<Lazy<BlobContainerClient>>("AvatarsContainer", (provider, key) =>
            {
                return new(() => provider.GetRequiredKeyedService<BlobServiceClient>("SignatureAccount").GetBlobContainerClient("avatars2024"));
            })
            .AddSlashCommand<OwnerDownloadAvatarsSlashCommand>()
            .ConfigureRequired<HeistOptions>(config, "Heist")
            .AddSingleton<IValidateOptions<HeistOptions>, HeistOptionsValidator>()
            .AddTransient<IHeistStatsRepository, HeistStatsPostgresRepository>()
            .AddTransient<IHeistConfigRepository, HeistConfigPostgresRepository>()
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
            .AddOptionParser<OptionalRpsShapeParser, RpsShape?>()
            .AddSlashCommand<RpsPlaySlashCommand>()
            .AddSlashCommand<RpsLeaderboardSlashCommand>()
            .AddSlashCommand<OwnerRewardYearbookActiveMembersSlashCommand>()
            .AddTransient<IRiskStatsRepository, RiskStatsPostgresRepository>()
            .AddOptionParser<OptionalRiskLevelParser, RiskLevel?>()
            .AddSlashCommand<RiskPlaySlashCommand>()
            .AddSlashCommand<RiskProfileSlashCommand>()
            .AddSlashCommand<RiskLeaderboardSlashCommand>()
            .AddTransient<IRollStatsRepository, RollStatsPostgresRepository>()
            .AddSlashCommand<RollPlaySlashCommand>()
            .AddSlashCommand<RollProfileSlashCommand>()
            .AddSlashCommand<RollLeaderboardSlashCommand>()
            .AddSlashCommand<UsernamesShowSlashCommand>()
            .AddSlashCommand<UsernamesVisibilitySlashCommand>()
            .AddTransient<ICouponRepository, CouponPostgresRepository>()
            .AddSlashCommand<CouponRedeemSlashCommand>()
            .AddSlashCommand<CouponShowSlashCommand>()
            .AddSlashCommand<OwnerAddCouponSlashCommand>()
            .AddSlashCommand<OwnerShowCouponsSlashCommand>()
            .AddTransient<IMemberActivityRepository, PostgresMemberActivityRepository>()
            .AddSlashCommand<RecapSlashCommand>()
            .AddTransient<IValentinesRepository, ValentinesPostgresRepository>()
            .AddSingleton<ValentineGiveawayDomainService>()
            .AddButtonHandler<ValentineGiveawayEnterHandler>()
            .AddSlashCommand<LoveReadySlashCommand>()
            .AddSlashCommand<LoveSpreadSlashCommand>()
            .AddSlashCommand<LoveHistorySlashCommand>()
            .AddTransient<IEggRepository, EggPostgresRepository>()
            .AddTransient<EggService>()
            .AddSlashCommand<EggVerifySlashCommand>()
            .AddSlashCommand<EggProfileSlashCommand>()
            .AddSlashCommand<EggStatusSlashCommand>()
            .AddSlashCommand<EggLeaderboardSlashCommand>()
            .AddSlashCommand<EggSetConfigSlashCommand>()
            .AddSlashCommand<EggRunSlashCommand>()
            ;

        services.AddHttpClient<ILastFmClient, InflatableLastFmClient>();
        services.AddHttpClient<IUrbanDictionaryClient, UrbanDictionaryClient>();
        services.AddHttpClient<GooglePlacesClient>();
        services.AddHttpClient<GooglePlacesNewClient>();
        services.AddHttpClient<IWeatherClient, PirateWeatherClient>();
        services.AddHttpClient<IWolframAlphaClient, WolframAlphaClient>();
        services.AddHttpClient<IHoroscopeClient, GaneshaSpeaksHoroscopeClient>();
        services.AddHttpClient<IImgurClient, ImgurHttpClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<ImgurOptions>>().CurrentValue;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", options.ClientId);
        });
    }
}
