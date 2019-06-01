using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Configuration;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Environment;
using TaylorBot.Net.EntityTracker.Program.Events;
using TaylorBot.Net.Core.Infrastructure.Configuration;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.QuickStart.Domain.Options;
using TaylorBot.Net.QuickStart.Domain;
using TaylorBot.Net.EntityTracker.Infrastructure.Username;
using TaylorBot.Net.EntityTracker.Domain.Username;
using TaylorBot.Net.EntityTracker.Infrastructure.User;
using TaylorBot.Net.EntityTracker.Domain.User;
using TaylorBot.Net.EntityTracker.Infrastructure.TextChannel;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.EntityTracker.Infrastructure.Guild;
using TaylorBot.Net.EntityTracker.Domain.Guild;
using TaylorBot.Net.EntityTracker.Infrastructure.GuildName;
using TaylorBot.Net.EntityTracker.Domain.GuildName;
using TaylorBot.Net.EntityTracker.Domain.Member;
using TaylorBot.Net.EntityTracker.Infrastructure.Member;
using TaylorBot.Net.EntityTracker.Domain.Options;

namespace TaylorBot.Net.EntityTracker.Program
{
    public class EntityTrackerProgram
    {
        public static async Task Main()
        {
            using (var services = new EntityTrackerProgram().ConfigureServices())
            {
                await new TaylorBotApplication(services).StartAsync();
            }
        }

        private ServiceProvider ConfigureServices()
        {
            var environment = TaylorBotEnvironment.CreateCurrent();

            var config = new ConfigurationBuilder()
                .AddTaylorBotApplicationConfiguration(environment)
                .AddDatabaseConnectionConfiguration(environment)
                .AddJsonFile(path: $"Settings/entityTracker.{environment}.json", optional: false)
                .AddJsonFile(path: $"Settings/quickStartEmbed.{environment}.json", optional: false)
                .Build();

            return new ServiceCollection()
                .AddTaylorBotApplicationServices(config)
                .AddTaylorBotApplicationLogging(config)
                .ConfigureDatabaseConnection(config)
                .ConfigureRequired<EntityTrackerOptions>(config, "EntityTracker")
                .ConfigureRequired<QuickStartEmbedOptions>(config, "QuickStartEmbed")
                .AddTransient<IShardReadyHandler, ShardReadyHandler>()
                .AddTransient<IJoinedGuildHandler, QuickStartJoinedGuildHandler>()
                .AddTransient<IJoinedGuildHandler, UsernameJoinedGuildHandler>()
                .AddTransient<IUserUpdatedHandler, UserUpdatedHandler>()
                .AddTransient<IGuildUpdatedHandler, GuildUpdatedHandler>()
                .AddTransient<QuickStartDomainService>()
                .AddTransient<EntityTrackerDomainService>()
                .AddTransient<IUserRepository, UserRepository>()
                .AddTransient<IUsernameRepository, UsernameRepository>()
                .AddTransient<ITextChannelRepository, TextChannelRepository>()
                .AddTransient<IGuildRepository, GuildRepository>()
                .AddTransient<IGuildNameRepository, GuildNameRepository>()
                .AddTransient<IMemberRepository, MemberRepository>()
                .BuildServiceProvider();
        }
    }
}
