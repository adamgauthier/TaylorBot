using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.CommandDisabling;

[Name("Command")]
[Group("command")]
[Alias("c")]
public class CommandModule(
    ICommandRunner commandRunner,
    IDisabledCommandRepository disabledCommandRepository,
    TaylorBotOwnerPrecondition ownerPrecondition) : TaylorBotModule
{
    [Command("enable-global")]
    public async Task<RuntimeResult> EnableGlobalAsync(
        [Remainder]
        ICommandRepository.Command command
    )
    {
        Command enableCommand = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                await disabledCommandRepository.EnableGloballyAsync(command.Name);

                return new EmbedResult(EmbedFactory.CreateSuccess($"Command `{command.Name}` has been enabled globally."));
            },
            Preconditions: [ownerPrecondition]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(enableCommand, context);

        return new TaylorBotResult(result, context);
    }

    [Command("disable-global")]
    public async Task<RuntimeResult> DisableGlobalAsync(
        ICommandRepository.Command command,
        [Remainder]
        string message
    )
    {
        Command disableCommand = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                if (command.Name.StartsWith("command", StringComparison.Ordinal))
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"Command `{command.Name}` can't be disabled because it's a framework command."));
                }

                var disabledMessage = await disabledCommandRepository.DisableGloballyAsync(command.Name, message);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"Command `{command.Name}` has been disabled globally with message '{disabledMessage}'."));
            },
            Preconditions: [ownerPrecondition]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(disableCommand, context);

        return new TaylorBotResult(result, context);
    }
}
