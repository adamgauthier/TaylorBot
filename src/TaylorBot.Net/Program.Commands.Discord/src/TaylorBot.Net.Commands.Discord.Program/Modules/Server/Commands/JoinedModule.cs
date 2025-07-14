using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

[Name("Joined 🚪")]
public class JoinedModule(ICommandRunner commandRunner, ServerJoinedSlashCommand serverJoinedCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("joined")]
    public async Task<RuntimeResult> JoinedAsync(
        [Remainder]
        IUserArgument<IGuildUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: ServerJoinedSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            serverJoinedCommand.Joined(new((IGuildUser)u), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rankjoined")]
    [Alias("rank joined")]
    public async Task<RuntimeResult> RankJoinedAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ServerTimelineSlashCommand.CommandName, IsRemoved: true));
}
