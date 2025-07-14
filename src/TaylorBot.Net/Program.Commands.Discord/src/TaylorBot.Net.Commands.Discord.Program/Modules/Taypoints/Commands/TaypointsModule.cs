using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

[Name("Taypoints 🪙")]
public class TaypointsModule(ICommandRunner commandRunner, TaypointsBalanceSlashCommand balanceCommand, TaypointsGiftSlashCommand giftCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("taypoints")]
    [Alias("points")]
    public async Task<RuntimeResult> BalanceAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: TaypointsBalanceSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            balanceCommand.Balance(new(u), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("ranktaypoints")]
    [Alias("rank taypoints", "rankpoints", "rank points")]
    public async Task<RuntimeResult> RankTaypointsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: TaypointsLeaderboardSlashCommand.CommandName, IsRemoved: true));

    [Command("gift")]
    [Alias("give")]
    public async Task<RuntimeResult> GiftAsync(
        string amount,
        [Remainder]
        IReadOnlyList<IMentionedUserNotAuthor<IUser>> users
    )
    {
        List<DiscordUser> trackedUsers = [];
        foreach (var user in users)
        {
            trackedUsers.Add(new(await user.GetTrackedUserAsync()));
        }

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: TaypointsGiftSlashCommand.CommandName));

        var result = await commandRunner.RunSlashCommandAsync(
            giftCommand.Gift(context, trackedUsers, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
