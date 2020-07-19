using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;

namespace TaylorBot.Net.Commands.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandApplication(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddTaylorBotApplicationServices(configuration)
                .AddSingleton(services)
                .AddSingleton(provider => new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async
                }))
                .AddTransient<IUserMessageReceivedHandler, CommandHandler>()
                .AddTransient<CommandExecutedHandler>()
                .AddTransient<CommandServiceLogger>()
                .AddTransient<IUserTracker, UserTracker>()
                .AddTransient<MentionedUserTypeReader<IUser>>()
                .AddTransient<CustomUserTypeReader<IUser>>()
                .AddTransient<MentionedUserTypeReader<IGuildUser>>()
                .AddTransient<CustomUserTypeReader<IGuildUser>>()
                .AddTransient<MentionedUserNotAuthorTypeReader<IGuildUser>>()
                .AddTransient<MentionedUserNotAuthorTypeReader<IUser>>()
                .AddTransient<MentionedUserNotAuthorOrClientTypeReader<IGuildUser>>()
                .AddTransient<CustomRoleTypeReader<IRole>>()
                .AddTransient<RoleNotEveryoneTypeReader<IRole>>()
                .AddTransient<CustomChannelTypeReader<IChannel>>()
                .AddTransient<PositiveInt32.Factory>()
                .AddTransient<WordTypeReader>();
        }
    }
}
