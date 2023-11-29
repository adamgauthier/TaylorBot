using Discord;
using Discord.Commands;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Commands
{
    [Name("Reward 🎖")]
    public class RewardModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly ITaypointRewardRepository _taypointRepository;

        public RewardModule(ICommandRunner commandRunner, ITaypointRewardRepository taypointRepository)
        {
            _commandRunner = commandRunner;
            _taypointRepository = taypointRepository;
        }

        [Command("reward")]
        [Summary("Rewards a specified amount of taypoints to pinged users.")]
        public async Task<RuntimeResult> RewardAsync(
            [Summary("How much taypoints do you want to reward each user?")]
            PositiveInt32 taypoints,
            [Remainder]
            [Summary("What users would you like to reward taypoints to (must be mentioned)?")]
            IReadOnlyCollection<IMentionedUserNotAuthor<IUser>> users
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

                    var rewardedUsers = await _taypointRepository.RewardUsersAsync(trackedUsers, taypoints.Parsed);

                    return new EmbedResult(new EmbedBuilder()
                        .WithUserAsAuthor(Context.User)
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Successfully rewarded {"taypoint".ToQuantity(taypoints.Parsed, TaylorBotFormats.BoldReadable)} to:"
                        }.Concat(rewardedUsers.Select(
                            u => $"{MentionUtils.MentionUser(u.UserId.Id)} - now has {u.NewTaypointCount.ToString(TaylorBotFormats.BoldReadable)}"
                        ))).Truncate(EmbedBuilder.MaxDescriptionLength))
                    .Build());
                },
                Preconditions: new[] { new TaylorBotOwnerPrecondition() }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
