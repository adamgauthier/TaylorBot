using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public class OwnerRewardSlashCommand(
    ITaypointRewardRepository taypointRepository,
    TaylorBotOwnerPrecondition ownerPrecondition) : ISlashCommand<OwnerRewardSlashCommand.Options>
{
    public static string CommandName => "owner reward";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(
        ParsedPositiveInteger amount,
        ParsedUser user1,
        ParsedUserOptional user2,
        ParsedUserOptional user3,
        ParsedUserOptional user4,
        ParsedUserOptional user5,
        ParsedUserOptional user6,
        ParsedUserOptional user7,
        ParsedUserOptional user8,
        ParsedUserOptional user9
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                List<DiscordUser> users = [options.user1.User];
                foreach (var optional in new[] {
                    options.user2.User,
                    options.user3.User,
                    options.user4.User,
                    options.user5.User,
                    options.user6.User,
                    options.user7.User,
                    options.user8.User,
                    options.user9.User,
                })
                {
                    if (optional != null)
                    {
                        users.Add(optional);
                    }
                }

                var amount = options.amount.Value;

                var rewardedUsers = await taypointRepository.RewardUsersAsync(users, amount);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Successfully rewarded {"taypoint".ToQuantity(amount, TaylorBotFormats.BoldReadable)} to:
                    {string.Join('\n', rewardedUsers.Select(
                        u => $"{MentionUtils.MentionUser(u.UserId.Id)} - now has {u.NewTaypointCount.ToString(TaylorBotFormats.BoldReadable)}"
                    )).Truncate(EmbedBuilder.MaxDescriptionLength)}
                    """
                ));
            },
            Preconditions: [
                ownerPrecondition
            ]
        ));
    }
}
