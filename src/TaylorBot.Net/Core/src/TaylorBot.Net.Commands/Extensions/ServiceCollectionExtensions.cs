using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;

namespace TaylorBot.Net.Commands.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTaylorBotCommandServices(this IServiceCollection services, IConfiguration configuration)
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
                .AddTransient<CommandServiceLogger>();
        }
    }
}
