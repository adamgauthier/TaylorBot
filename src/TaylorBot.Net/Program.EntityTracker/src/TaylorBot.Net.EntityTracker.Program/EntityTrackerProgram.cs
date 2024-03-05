using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Infrastructure;
using TaylorBot.Net.EntityTracker.Program.Events;
using TaylorBot.Net.MemberLogging.Domain;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;
using TaylorBot.Net.MemberLogging.Domain.Options;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;
using TaylorBot.Net.MemberLogging.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MessagesTracker.Infrastructure;
using TaylorBot.Net.MinutesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Domain.Options;
using TaylorBot.Net.MinutesTracker.Infrastructure;
using TaylorBot.Net.QuickStart.Domain;
using TaylorBot.Net.QuickStart.Domain.Options;


var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((hostBuilderContext, appConfig) =>
    {
        var env = hostBuilderContext.HostingEnvironment;

        appConfig
            .AddTaylorBotApplication(env)
            .AddDatabaseConnection(env)
            .AddRedisConnection(env)
            .AddEntityTracker(env)
            .AddJsonFile(path: "Settings/memberLogging.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: "Settings/quickStartEmbed.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: "Settings/minutesTracker.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: "Settings/messagesTracker.json", optional: false, reloadOnChange: true);

        appConfig.AddEnvironmentVariables("TaylorBot_");
    })
    .ConfigureServices((hostBuilderContext, services) =>
    {
        var config = hostBuilderContext.Configuration;
        services
            .AddHostedService<TaylorBotHostedService>()
            .AddTaylorBotApplicationServices(config, hostBuilderContext.HostingEnvironment)
            .AddPostgresConnection(config, withTracing: false)
            .AddRedisConnection(config)
            .AddEntityTrackerInfrastructure(config)
            .ConfigureRequired<QuickStartEmbedOptions>(config, "QuickStartEmbed")
            .AddTransient<QuickStartDomainService>()
            .AddTransient<QuickStartChannelFinder>()
            .ConfigureRequired<MemberLoggingOptions>(config, "MemberLogging")
            .AddTransient<MemberLogChannelFinder>()
            .AddTransient<GuildMemberJoinedLoggerService>()
            .AddTransient<GuildMemberJoinedEmbedFactory>()
            .AddTransient<IMemberLoggingChannelRepository, MemberLoggingChannelRepository>()
            .AddTransient<IShardReadyHandler, ShardReadyHandler>()
            .AddTransient<IJoinedGuildHandler, QuickStartJoinedGuildHandler>()
            .AddTransient<IJoinedGuildHandler, UsernameJoinedGuildHandler>()
            .AddTransient<IUserUpdatedHandler, UserUpdatedHandler>()
            .AddTransient<IGuildUpdatedHandler, GuildUpdatedHandler>()
            .AddTransient<IGuildUserJoinedHandler, GuildUserJoinedHandler>()
            .AddTransient<IGuildUserLeftHandler, GuildUserLeftHandler>()
            .AddTransient<ITextChannelCreatedHandler, TextChannelCreatedHandler>()
            .ConfigureRequired<MinutesTrackerOptions>(config, "MinutesTracker")
            .AddTransient<IUserMessageReceivedHandler, UserMessageReceivedHandler>()
            .AddTransient<SingletonTaskRunner>()
            .AddTransient<IMinuteRepository, MinutePostgresRepository>()
            .AddTransient<MinutesTrackerDomainService>()
            .ConfigureRequired<MessagesTrackerOptions>(config, "MessagesTracker")
            .AddTransient<IMessageRepository, MessagesPostgresRepository>()
            .AddTransient<ITextChannelMessageCountRepository, TextChannelMessageCountPostgresRepository>()
            .AddTransient<IGuildUserLastSpokeRepository, GuildUserLastSpokePostgresRepository>()
            .AddTransient<WordCounter>()
            .AddTransient<MessagesTrackerDomainService>();
    })
    .Build();

await host.RunAsync();
