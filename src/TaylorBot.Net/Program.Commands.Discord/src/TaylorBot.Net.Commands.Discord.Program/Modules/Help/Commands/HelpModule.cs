using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Help.Commands;

[Name("Help")]
public class HelpModule(ICommandRunner commandRunner, HelpSlashCommand helpCommand) : TaylorBotModule
{
    [Command("help")]
    [Alias("botinfo", "version", "invite")]
    [Summary("Lists help and information for a module's commands.")]
    public async Task<RuntimeResult> HelpAsync(
        [Remainder]
        string? _ = null)
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var response = await helpCommand.GetHelpResponseAsync(context, isSlashCommand: false);

        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(response.Content.Embeds.Single())));

        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
