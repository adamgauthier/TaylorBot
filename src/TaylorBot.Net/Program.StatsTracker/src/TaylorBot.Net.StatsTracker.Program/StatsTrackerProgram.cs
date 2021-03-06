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
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MessagesTracker.Infrastructure;
using TaylorBot.Net.MinutesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Domain.Options;
using TaylorBot.Net.MinutesTracker.Infrastructure;
using TaylorBot.Net.StatsTracker.Program.Events;


var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((hostBuilderContext, appConfig) =>
    {
        var env = hostBuilderContext.HostingEnvironment;

        appConfig
            .AddTaylorBotApplication(env)
            .AddDatabaseConnection(env)
            .AddRedisConnection(env)
            .AddEntityTracker(env)
            .AddJsonFile(path: "Settings/minutesTracker.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: "Settings/messagesTracker.json", optional: false, reloadOnChange: true);

        appConfig.AddEnvironmentVariables("TaylorBot_");
    })
    .ConfigureServices((hostBuilderContext, services) =>
    {
        var config = hostBuilderContext.Configuration;
        services
            .AddHostedService<TaylorBotHostedService>()
            .AddTaylorBotApplicationServices(config)
            .AddPostgresConnection(config)
            .AddRedisConnection(config)
            .AddEntityTrackerInfrastructure(config)
            .ConfigureRequired<MinutesTrackerOptions>(config, "MinutesTracker")
            .AddTransient<IShardReadyHandler, ReadyHandler>()
            .AddTransient<IUserMessageReceivedHandler, UserMessageReceivedHandler>()
            .AddTransient<SingletonTaskRunner>()
            .AddTransient<IMinuteRepository, MinutesRepository>()
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
