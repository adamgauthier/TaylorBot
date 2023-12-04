using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Program;

namespace TaylorBot.Net.Commands;

public interface ITaylorBotTypeReader
{
    Type ArgumentType { get; }
}

public class TaylorBotCommandHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaylorBotCommandHostedService> _logger;
    private readonly TaylorBotHostedService _taylorBotHostedService;

    public TaylorBotCommandHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILogger<TaylorBotCommandHostedService>>();
        _taylorBotHostedService = new TaylorBotHostedService(serviceProvider);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var commandService = _serviceProvider.GetRequiredService<CommandService>();
        var commandExecutedHandler = _serviceProvider.GetRequiredService<CommandExecutedHandler>();
        var commandServiceLogger = _serviceProvider.GetRequiredService<CommandServiceLogger>();

        commandService.Log += commandServiceLogger.OnCommandServiceLogAsync;
        commandService.CommandExecuted += commandExecutedHandler.OnCommandExecutedAsync;

        _logger.LogInformation("Adding type readers");

        commandService.AddTypeReader<IUserArgument<IUser>>(_serviceProvider.GetRequiredService<CustomUserTypeReader<IUser>>());
        commandService.AddTypeReader<IUserArgument<IGuildUser>>(_serviceProvider.GetRequiredService<CustomUserTypeReader<IGuildUser>>());
        commandService.AddTypeReader<IMentionedUser<IUser>>(_serviceProvider.GetRequiredService<MentionedUserTypeReader<IUser>>());
        commandService.AddTypeReader<IMentionedUser<IGuildUser>>(_serviceProvider.GetRequiredService<MentionedUserTypeReader<IGuildUser>>());
        commandService.AddTypeReader<IMentionedUserNotAuthor<IUser>>(_serviceProvider.GetRequiredService<MentionedUserNotAuthorTypeReader<IUser>>());
        commandService.AddTypeReader<IReadOnlyList<IMentionedUserNotAuthor<IUser>>>(_serviceProvider.GetRequiredService<MentionedUsersNotAuthorTypeReader<IUser>>());
        commandService.AddTypeReader<IMentionedUserNotAuthor<IGuildUser>>(_serviceProvider.GetRequiredService<MentionedUserNotAuthorTypeReader<IGuildUser>>());
        commandService.AddTypeReader<IMentionedUserNotAuthorOrClient<IGuildUser>>(_serviceProvider.GetRequiredService<MentionedUserNotAuthorOrClientTypeReader<IGuildUser>>());
        commandService.AddTypeReader<RoleArgument<IRole>>(_serviceProvider.GetRequiredService<CustomRoleTypeReader<IRole>>());
        commandService.AddTypeReader<RoleNotEveryoneArgument<IRole>>(_serviceProvider.GetRequiredService<RoleNotEveryoneTypeReader<IRole>>());
        commandService.AddTypeReader<IChannelArgument<IChannel>>(_serviceProvider.GetRequiredService<CustomChannelTypeReader<IChannel>>());
        commandService.AddTypeReader<IChannelArgument<ITextChannel>>(_serviceProvider.GetRequiredService<CustomChannelTypeReader<ITextChannel>>());
        commandService.AddTypeReader<PositiveInt32>(new ConstrainedIntTypeReader<PositiveInt32.Factory>(PositiveInt32.Min));
        commandService.AddTypeReader<Word>(_serviceProvider.GetRequiredService<WordTypeReader>());
        commandService.AddTypeReader<ICommandRepository.Command>(_serviceProvider.GetRequiredService<CommandTypeReader>());

        foreach (var typeReader in _serviceProvider.GetServices<ITaylorBotTypeReader>())
        {
            commandService.AddTypeReader(typeReader.ArgumentType, (TypeReader)typeReader);
        }

        _logger.LogInformation("Adding command modules");

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
