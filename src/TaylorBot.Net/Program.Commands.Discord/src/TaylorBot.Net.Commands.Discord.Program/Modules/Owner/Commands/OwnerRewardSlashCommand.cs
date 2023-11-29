using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public class OwnerRewardSlashCommand(ITaypointRewardRepository taypointRepository) : ISlashCommand<OwnerRewardSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("owner reward");

    public record Options(
        ParsedPositiveInteger amount,
        ParsedUser user1,
        ParsedOptionalUser user2,
        ParsedOptionalUser user3,
        ParsedOptionalUser user4,
        ParsedOptionalUser user5,
        ParsedOptionalUser user6,
        ParsedOptionalUser user7,
        ParsedOptionalUser user8,
        ParsedOptionalUser user9
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                List<IUser> users = [options.user1.User];
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
                    string.Join('\n', new[] {
                        $"Successfully rewarded {"taypoint".ToQuantity(amount, TaylorBotFormats.BoldReadable)} to:"
                    }.Concat(rewardedUsers.Select(
                        u => $"{MentionUtils.MentionUser(u.UserId.Id)} - now has {u.NewTaypointCount.ToString(TaylorBotFormats.BoldReadable)}"
                    ))).Truncate(EmbedBuilder.MaxDescriptionLength)
                ));
            },
            Preconditions: new ICommandPrecondition[] {
                new TaylorBotOwnerPrecondition()
            }
        ));
    }
}
