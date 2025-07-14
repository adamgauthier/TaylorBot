using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Help.Commands;

[Name("Help")]
public class HelpModule(ICommandRunner commandRunner, HelpSlashCommand helpCommand) : TaylorBotModule
{
    [Command("help")]
    [Alias("botinfo", "version", "invite")]
    public async Task<RuntimeResult> HelpAsync(
        [Remainder]
        string? _ = null)
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: HelpSlashCommand.CommandName));
        var response = await helpCommand.GetHelpResponseAsync(context);

        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(response.Content.Embeds.Single())));

        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
