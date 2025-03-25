using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.CommandDisabling;

[Name("Command")]
[Group("command")]
[Alias("c")]
public class CommandModule(
    ICommandRunner commandRunner,
    IDisabledCommandRepository disabledCommandRepository,
    TaylorBotOwnerPrecondition ownerPrecondition) : TaylorBotModule
{
    private static readonly string[] GuardedModuleNames = ["framework", "command"];

    [Command("enable-global")]
    [Summary("Enables a command globally.")]
    public async Task<RuntimeResult> EnableGlobalAsync(
        [Summary("What command would you like to enable?")]
        [Remainder]
        ICommandRepository.Command command
    )
    {
        Command enableCommand = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                await disabledCommandRepository.EnableGloballyAsync(command.Name);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription($"Command `{command.Name}` has been enabled globally.")
                .Build());
            },
            Preconditions: [ownerPrecondition]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(enableCommand, context);

        return new TaylorBotResult(result, context);
    }

    [Command("disable-global")]
    [Summary("Disables a command globally.")]
    public async Task<RuntimeResult> DisableGlobalAsync(
        [Summary("What command would you like to disable?")]
        ICommandRepository.Command command,
        [Remainder]
        [Summary("What message should be shown to someone using the disabled command?")]
        string message
    )
    {
        Command disableCommand = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                EmbedBuilder embed = new();

                if (GuardedModuleNames.Contains(command.ModuleName.ToLowerInvariant()))
                {
                    return new EmbedResult(embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription($"Command `{command.Name}` can't be disabled because it's a framework command.")
                    .Build());
                }

                var disabledMessage = await disabledCommandRepository.DisableGloballyAsync(command.Name, message);

                return new EmbedResult(embed
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription($"Command `{command.Name}` has been disabled globally with message '{disabledMessage}'.")
                .Build());
            },
            Preconditions: [ownerPrecondition]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(disableCommand, context);

        return new TaylorBotResult(result, context);
    }
}
