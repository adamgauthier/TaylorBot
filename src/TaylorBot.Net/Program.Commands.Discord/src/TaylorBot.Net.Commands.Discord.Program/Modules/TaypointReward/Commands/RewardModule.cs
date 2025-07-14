using Discord;
using Discord.Commands;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Commands;

[Name("Reward 🎖")]
public class RewardModule(
    ICommandRunner commandRunner,
    ITaypointRewardRepository taypointRepository,
    TaylorBotOwnerPrecondition ownerPrecondition) : TaylorBotModule
{
    [Command("reward")]
    public async Task<RuntimeResult> RewardAsync(
        PositiveInt32 taypoints,
        [Remainder]
        IReadOnlyList<IMentionedUserNotAuthor<IUser>> users
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                var trackedUsers = new List<IUser>();
                foreach (var user in users)
                {
                    trackedUsers.Add(await user.GetTrackedUserAsync());
                }

                var rewardedUsers = await taypointRepository.RewardUsersAsync([.. trackedUsers.Select(u => new DiscordUser(u))], taypoints.Parsed);

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
            Preconditions: [ownerPrecondition]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
