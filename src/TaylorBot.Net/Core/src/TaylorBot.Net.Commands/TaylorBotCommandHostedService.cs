using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Modules;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Program;

namespace TaylorBot.Net.Commands
{
    public class TaylorBotCommandHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TaylorBotHostedService _taylorBotHostedService;

        public TaylorBotCommandHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _taylorBotHostedService = new TaylorBotHostedService(serviceProvider);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var commandService = _serviceProvider.GetRequiredService<CommandService>();
            var commandExecutedHandler = _serviceProvider.GetRequiredService<CommandExecutedHandler>();
            var commandServiceLogger = _serviceProvider.GetRequiredService<CommandServiceLogger>();

            commandService.Log += commandServiceLogger.OnCommandServiceLogAsync;
            commandService.CommandExecuted += commandExecutedHandler.OnCommandExecutedAsync;

            commandService.AddTypeReader<IUserArgument<IUser>>(_serviceProvider.GetRequiredService<CustomUserTypeReader<IUser>>());
            commandService.AddTypeReader<IUserArgument<IGuildUser>>(_serviceProvider.GetRequiredService<CustomUserTypeReader<IGuildUser>>());
            commandService.AddTypeReader<IMentionedUser<IUser>>(_serviceProvider.GetRequiredService<MentionedUserTypeReader<IUser>>());
            commandService.AddTypeReader<IMentionedUser<IGuildUser>>(_serviceProvider.GetRequiredService<MentionedUserTypeReader<IGuildUser>>());
            commandService.AddTypeReader<IMentionedUserNotAuthor<IUser>>(_serviceProvider.GetRequiredService<MentionedUserNotAuthorTypeReader<IUser>>());
            commandService.AddTypeReader<IMentionedUserNotAuthor<IGuildUser>>(_serviceProvider.GetRequiredService<MentionedUserNotAuthorTypeReader<IGuildUser>>());
            commandService.AddTypeReader<IRole>(_serviceProvider.GetRequiredService<CustomRoleTypeReader<IRole>>(), replaceDefault: true);

            await commandService.AddModuleAsync<HelpModule>(_serviceProvider);
            await commandService.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _serviceProvider
            );

            await _taylorBotHostedService.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _taylorBotHostedService.StopAsync(cancellationToken);
        }
    }
}
