using Discord;
using Discord.Commands;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Commands;

[Name("Reward 🎖")]
public class RewardModule(ICommandRunner commandRunner, ITaypointRewardRepository taypointRepository) : TaylorBotModule
{
    [Command("reward")]
    [Summary("Rewards a specified amount of taypoints to pinged users.")]
    public async Task<RuntimeResult> RewardAsync(
        [Summary("How much taypoints do you want to reward each user?")]
        PositiveInt32 taypoints,
        [Remainder]
        [Summary("What users would you like to reward taypoints to (must be mentioned)?")]
        IReadOnlyList<IMentionedUserNotAuthor<IUser>> users
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                var trackedUsers = new List<IUser>();
                foreach (var user in users)
                {
                    trackedUsers.Add(await user.GetTrackedUserAsync());
                }

                var rewardedUsers = await taypointRepository.RewardUsersAsync(trackedUsers, taypoints.Parsed);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Successfully rewarded {"taypoint".ToQuantity(taypoints.Parsed, TaylorBotFormats.BoldReadable)} to:
                        {string.Join('\n', rewardedUsers.Select(
                            u => $"{MentionUtils.MentionUser(u.UserId)} - now has {u.NewTaypointCount.ToString(TaylorBotFormats.BoldReadable)}"
                        ))}
                        """.Truncate(EmbedBuilder.MaxDescriptionLength))
                .Build());
            },
            Preconditions: new[] { new TaylorBotOwnerPrecondition() }
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
