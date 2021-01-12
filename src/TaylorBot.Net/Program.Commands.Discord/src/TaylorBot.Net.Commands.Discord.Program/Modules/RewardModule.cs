using Discord;
using Discord.Commands;
using Humanizer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.TaypointReward.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Reward 🎖")]
    public class RewardModule : TaylorBotModule
    {
        private readonly ITaypointRewardRepository _taypointRepository;

        public RewardModule(ITaypointRewardRepository taypointRepository)
        {
            _taypointRepository = taypointRepository;
        }

        [RequireTaylorBotOwner]
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
            var trackedUsers = new List<IUser>();
            foreach (var user in users)
            {
                trackedUsers.Add(await user.GetTrackedUserAsync());
            }

            var rewardedUsers = await _taypointRepository.RewardUsersAsync(trackedUsers, taypoints.Parsed);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(string.Join('\n', new[] {
                    $"Successfully rewarded {"taypoint".ToQuantity(taypoints.Parsed, TaylorBotFormats.BoldReadable)} to:"
                }.Concat(rewardedUsers.Select(
                    u => $"{MentionUtils.MentionUser(u.UserId.Id)} - now has {u.NewTaypointCount.ToString(TaylorBotFormats.BoldReadable)}"
                ))).Truncate(2048))
            .Build());
        }
    }
}
