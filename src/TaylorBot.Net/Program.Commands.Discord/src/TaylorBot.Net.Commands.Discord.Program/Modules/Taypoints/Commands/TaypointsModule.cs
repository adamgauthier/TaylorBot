using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

[Name("Taypoints 🪙")]
public class TaypointsModule(ICommandRunner commandRunner, TaypointsBalanceSlashCommand balanceCommand, TaypointsGiftSlashCommand giftCommand) : TaylorBotModule
{
    [Command("taypoints")]
    [Alias("points")]
    [Summary("Show the current taypoint balance of a user")]
    public async Task<RuntimeResult> BalanceAsync(
        [Summary("What user would you like to see the balance of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(
            balanceCommand.Balance(u),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("ranktaypoints")]
    [Alias("rank taypoints", "rankpoints", "rank points")]
    [Summary("This command has been moved to </taypoints leaderboard:1103846727880028180>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankTaypointsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </taypoints leaderboard:1103846727880028180> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("gift")]
    [Alias("give")]
    [Summary("Gifts a specified amount of taypoints to pinged users.")]
    public async Task<RuntimeResult> GiftAsync(
        [Summary("How much of your taypoints do you want to gift?")]
        string amount,
        [Summary("What users would you like to gift taypoints to (must be mentioned)?")]
        [Remainder]
        IReadOnlyList<IMentionedUserNotAuthor<IUser>> users
    )
    {
        List<IUser> trackedUsers = [];
        foreach (var user in users)
        {
            trackedUsers.Add(await user.GetTrackedUserAsync());
        }

        var context = DiscordNetContextMapper.MapToRunContext(Context);

        var result = await commandRunner.RunAsync(
            giftCommand.Gift(context, Context.User, trackedUsers, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
