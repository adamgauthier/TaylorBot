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

public class TaylorBotCommandHostedService(IServiceProvider services, ILogger<TaylorBotCommandHostedService> logger, TaylorBotHostedService taylorBotHostedService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var commandService = services.GetRequiredService<CommandService>();
        var commandExecutedHandler = services.GetRequiredService<CommandExecutedHandler>();
        var commandServiceLogger = services.GetRequiredService<CommandServiceLogger>();

        commandService.Log += commandServiceLogger.OnCommandServiceLogAsync;
        commandService.CommandExecuted += commandExecutedHandler.OnCommandExecutedAsync;

        logger.LogInformation("Adding type readers");

        commandService.AddTypeReader<IUserArgument<IUser>>(services.GetRequiredService<CustomUserTypeReader<IUser>>());
        commandService.AddTypeReader<IUserArgument<IGuildUser>>(services.GetRequiredService<CustomUserTypeReader<IGuildUser>>());
        commandService.AddTypeReader<IMentionedUser<IUser>>(services.GetRequiredService<MentionedUserTypeReader<IUser>>());
        commandService.AddTypeReader<IMentionedUser<IGuildUser>>(services.GetRequiredService<MentionedUserTypeReader<IGuildUser>>());
        commandService.AddTypeReader<IMentionedUserNotAuthor<IUser>>(services.GetRequiredService<MentionedUserNotAuthorTypeReader<IUser>>());
        commandService.AddTypeReader<IReadOnlyList<IMentionedUserNotAuthor<IUser>>>(services.GetRequiredService<MentionedUsersNotAuthorTypeReader<IUser>>());
        commandService.AddTypeReader<IMentionedUserNotAuthor<IGuildUser>>(services.GetRequiredService<MentionedUserNotAuthorTypeReader<IGuildUser>>());
        commandService.AddTypeReader<IMentionedUserNotAuthorOrClient<IGuildUser>>(services.GetRequiredService<MentionedUserNotAuthorOrClientTypeReader<IGuildUser>>());
        commandService.AddTypeReader<RoleArgument<IRole>>(services.GetRequiredService<CustomRoleTypeReader<IRole>>());
        commandService.AddTypeReader<RoleNotEveryoneArgument<IRole>>(services.GetRequiredService<RoleNotEveryoneTypeReader<IRole>>());
        commandService.AddTypeReader<IChannelArgument<IChannel>>(services.GetRequiredService<CustomChannelTypeReader<IChannel>>());
        commandService.AddTypeReader<IChannelArgument<ITextChannel>>(services.GetRequiredService<CustomChannelTypeReader<ITextChannel>>());
        commandService.AddTypeReader<PositiveInt32>(new ConstrainedIntTypeReader<PositiveInt32.Factory>(PositiveInt32.Min));
        commandService.AddTypeReader<Word>(services.GetRequiredService<WordTypeReader>());
        commandService.AddTypeReader<ICommandRepository.Command>(services.GetRequiredService<CommandTypeReader>());

        foreach (var typeReader in services.GetServices<ITaylorBotTypeReader>())
        {
            commandService.AddTypeReader(typeReader.ArgumentType, (TypeReader)typeReader);
        }

        logger.LogInformation("Adding command modules");

        await commandService.AddModulesAsync(
            assembly: Assembly.GetEntryAssembly(),
            services: services
        );

        await taylorBotHostedService.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return taylorBotHostedService.StopAsync(cancellationToken);
    }
}
