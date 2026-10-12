using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public class OwnerRewardUserSlashCommand(
    ITaypointRewardRepository taypointRepository,
    IUserTracker userTracker,
    TaylorBotOwnerPrecondition ownerPrecondition) : ISlashCommand<OwnerRewardUserSlashCommand.Options>
{
    public static string CommandName => "owner reward-user";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedPositiveInteger amount, ParsedUser user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await userTracker.TrackUserFromArgumentAsync(options.user.User);
                var amount = options.amount.Value;

                var rewarded = await taypointRepository.RewardUsersAsync([options.user.User], amount);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    {OwnerRewardMessage.FormatHeader(context.User, amount, rewarded)}
                    ### Balances Updated
                    {OwnerRewardMessage.FormatBalance(rewarded.Single(), amount)}
                    """
                ));
            },
            Preconditions: [
                ownerPrecondition
            ]
        ));
    }
}
