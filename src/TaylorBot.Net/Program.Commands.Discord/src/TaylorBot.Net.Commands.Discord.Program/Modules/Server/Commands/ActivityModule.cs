using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

[Name("Activity 📚")]
public class ActivityModule(ICommandRunner commandRunner, PrefixedCommandRunner prefixedCommandRunner, ServerMessagesSlashCommand serverMessagesCommand, ServerMinutesSlashCommand serverMinutesCommand) : TaylorBotModule
{
    [Command("messages")]
    public async Task<RuntimeResult> MessagesAsync(
        [Remainder]
        IUserArgument<IGuildUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: ServerMessagesSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            serverMessagesCommand.Messages(new((IGuildUser)u)),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rankmessages")]
    [Alias("rank messages")]
    public async Task<RuntimeResult> RankMessagesAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ServerLeaderboardSlashCommand.CommandName, IsRemoved: true));

    [Command("minutes")]
    public async Task<RuntimeResult> MinutesAsync(
        [Remainder]
        IUserArgument<IGuildUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: ServerMinutesSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            serverMinutesCommand.Minutes(new((IGuildUser)u), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rankminutes")]
    [Alias("rank minutes")]
    public async Task<RuntimeResult> RankMinutesAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ServerLeaderboardSlashCommand.CommandName, IsRemoved: true));
}
