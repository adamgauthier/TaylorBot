using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.EntityTracker.Domain.Guild;
using TaylorBot.Net.EntityTracker.Domain.GuildName;
using TaylorBot.Net.EntityTracker.Domain.Member;
using TaylorBot.Net.EntityTracker.Domain.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.EntityTracker.Domain.User;
using TaylorBot.Net.EntityTracker.Domain.Username;
using TaylorBot.Net.EntityTracker.Infrastructure.Guild;
using TaylorBot.Net.EntityTracker.Infrastructure.GuildName;
using TaylorBot.Net.EntityTracker.Infrastructure.Member;
using TaylorBot.Net.EntityTracker.Infrastructure.TextChannel;
using TaylorBot.Net.EntityTracker.Infrastructure.User;
using TaylorBot.Net.EntityTracker.Infrastructure.Username;
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
            var environment = TaylorBotEnvironment.CreateCurrent();

            var host = new HostBuilder()
                .UseEnvironment(environment.ToString())
                .ConfigureAppConfiguration((hostBuilderContext, appConfig) =>
                {
                    var env = hostBuilderContext.HostingEnvironment.EnvironmentName;
                    appConfig
                        .AddTaylorBotApplicationConfiguration(environment)
                        .AddDatabaseConnectionConfiguration(environment)
                        .AddJsonFile(path: $"Settings/entityTracker.{env}.json", optional: false)
                        .AddJsonFile(path: $"Settings/quickStartEmbed.{env}.json", optional: false)
                        .AddJsonFile(path: $"Settings/memberLogging.{env}.json", optional: false);
                })
                .ConfigureLogging((hostBuilderContext, logging) =>
                {
                    logging.AddTaylorBotApplicationLogging(hostBuilderContext.Configuration);
                })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    var config = hostBuilderContext.Configuration;
                    services
                        .AddHostedService<TaylorBotHostedService>()
                        .AddTaylorBotApplicationServices(config)
                        .ConfigureDatabaseConnection(config)
                        .ConfigureRequired<EntityTrackerOptions>(config, "EntityTracker")
                        .ConfigureRequired<QuickStartEmbedOptions>(config, "QuickStartEmbed")
                        .ConfigureRequired<MemberLoggingOptions>(config, "MemberLogging")
                        .AddTransient<IShardReadyHandler, ShardReadyHandler>()
                        .AddTransient<IJoinedGuildHandler, QuickStartJoinedGuildHandler>()
                        .AddTransient<IJoinedGuildHandler, UsernameJoinedGuildHandler>()
                        .AddTransient<IUserUpdatedHandler, UserUpdatedHandler>()
                        .AddTransient<IGuildUpdatedHandler, GuildUpdatedHandler>()
                        .AddTransient<IGuildUserJoinedHandler, GuildUserJoinedHandler>()
                        .AddTransient<IGuildUserLeftHandler, GuildUserLeftHandler>()
                        .AddTransient<ITextChannelCreatedHandler, TextChannelCreatedHandler>()
                        .AddTransient<QuickStartDomainService>()
                        .AddTransient<UsernameTrackerDomainService>()
                        .AddTransient<EntityTrackerDomainService>()
                        .AddTransient<MemberLogChannelFinder>()
                        .AddTransient<GuildMemberJoinedLoggerService>()
                        .AddTransient<GuildMemberJoinedEmbedFactory>()
                        .AddTransient<IUserRepository, UserRepository>()
                        .AddTransient<IUsernameRepository, UsernameRepository>()
                        .AddTransient<ITextChannelRepository, TextChannelRepository>()
                        .AddTransient<IGuildRepository, GuildRepository>()
                        .AddTransient<IGuildNameRepository, GuildNameRepository>()
                        .AddTransient<IMemberRepository, MemberRepository>()
                        .AddTransient<IMemberLoggingChannelRepository, MemberLoggingChannelRepository>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
