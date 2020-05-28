using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.EntityTracker.Infrastructure;
using TaylorBot.Net.EntityTracker.Program.Events;
using TaylorBot.Net.MemberLogging.Domain;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;
using TaylorBot.Net.MemberLogging.Domain.Options;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;
using TaylorBot.Net.MemberLogging.Infrastructure;
using TaylorBot.Net.QuickStart.Domain;
using TaylorBot.Net.QuickStart.Domain.Options;

namespace TaylorBot.Net.EntityTracker.Program
{
    public class EntityTrackerProgram
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
                        .AddEntityTracker()
                        .AddJsonFile(path: "Settings/memberLogging.json", optional: false)
                        .AddJsonFile(path: "Settings/quickStartEmbed.json", optional: false);

                    appConfig.AddEnvironmentVariables("TaylorBot_");
                })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    var config = hostBuilderContext.Configuration;
                    services
                        .AddHostedService<TaylorBotHostedService>()
                        .AddTaylorBotApplicationServices(config)
                        .AddPostgresConnection(config)
                        .AddEntityTrackerInfrastructure(config)
                        .ConfigureRequired<QuickStartEmbedOptions>(config, "QuickStartEmbed")
                        .AddTransient<QuickStartDomainService>()
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
                        .AddTransient<ITextChannelCreatedHandler, TextChannelCreatedHandler>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
