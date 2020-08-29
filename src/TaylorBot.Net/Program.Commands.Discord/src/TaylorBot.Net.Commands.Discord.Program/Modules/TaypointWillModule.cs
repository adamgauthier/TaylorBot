using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("TaypointWill")]
    [Group("taypointwill")]
    public class TaypointWillModule : TaylorBotModule
    {
        private readonly IOptionsMonitor<TaypointWillOptions> _options;
        private readonly ITaypointWillRepository _taypointWillRepository;

        public TaypointWillModule(IOptionsMonitor<TaypointWillOptions> options, ITaypointWillRepository taypointWillRepository)
        {
            _options = options;
            _taypointWillRepository = taypointWillRepository;
        }

        [Priority(-1)]
        [Command]
        [Summary("Displays a user's taypoint will.")]
        public async Task<RuntimeResult> GetAsync(
            [Summary("What user would you like to see the taypoint will of?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var u = user == null ? Context.User : await user.GetTrackedUserAsync();

            var will = await _taypointWillRepository.GetWillAsync(owner: u);

            var embed = new EmbedBuilder()
                .WithUserAsAuthor(u);

            if (will != null)
            {
                var days = _options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;
                var beneficiary = MentionUtils.MentionUser(will.BeneficiaryUserId.Id);
                embed
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"{u.Username}'s taypoint will has a beneficiary: {beneficiary}.",
                        $"If they are inactive for {"day".ToQuantity(days)} in all servers I'm in, {beneficiary} can claim all their taypoints with `{Context.CommandPrefix}taypointwill claim`.",
                        $"Use `{Context.CommandPrefix}taypointwill clear` to remove your beneficiary."
                    }));
            }
            else
            {
                embed
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"{u.Username}'s taypoint will has no beneficiary. If they ever become inactive, their taypoints won't be used!",
                        $"Add a beneficiary to your taypoint will with `{Context.CommandPrefix}taypointwill add`!"
                    }));
            }

            return new TaylorBotEmbedResult(embed.Build());
        }

        [Command("add")]
        [Summary("Adds a user as beneficiary to your taypoint will.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("What user would you like to add to your taypoint will?")]
            [Remainder]
            IMentionedUserNotAuthor<IUser> mentionedUser
        )
        {
            var user = await mentionedUser.GetTrackedUserAsync();

            var result = await _taypointWillRepository.AddWillAsync(owner: Context.User, beneficiary: user);

            var embed = new EmbedBuilder()
                .WithUserAsAuthor(Context.User);

            switch (result)
            {
                case WillAddedResult _:
                    var days = _options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;
                    embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Successfully added {user.Mention} as beneficiary to your taypoint will.",
                            $"If you become inactive for more than {"day".ToQuantity(days)} in all servers we share, {user.Mention} will be able to claim **all your taypoints** with `{Context.CommandPrefix}taypointwill claim`.",
                            $"If you change your mind at any time, you can use `{Context.CommandPrefix}taypointwill clear` to remove them."
                        }));
                    break;
                case WillNotAddedResult willNotAdded:
                    embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Can't add {user.Mention} as beneficiary to your taypoint will because it is set to {MentionUtils.MentionUser(willNotAdded.CurrentBeneficiaryId.Id)}.",
                            $"If you want to change your beneficiary, you first need to use `{Context.CommandPrefix}taypointwill clear`.",
                        }));
                    break;
            }

            return new TaylorBotEmbedResult(embed.Build());
        }

        [Command("clear")]
        [Summary("Clears your taypoint will.")]
        public async Task<RuntimeResult> ClearAsync()
        {
            var result = await _taypointWillRepository.RemoveWillWithOwnerAsync(Context.User);

            var embed = new EmbedBuilder()
                .WithUserAsAuthor(Context.User);

            switch (result)
            {
                case WillRemovedResult willRemoved:
                    embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Your taypoint will with {MentionUtils.MentionUser(willRemoved.RemovedBeneficiaryId.Id)} has been cleared.",
                            $"You can add a beneficiary again with `{Context.CommandPrefix}taypointwill add`."
                        }));
                    break;
                case WillNotRemovedResult _:
                    embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"You don't have a taypoint will to clear.",
                            $"Start adding a beneficiary with `{Context.CommandPrefix}taypointwill add`."
                        }));
                    break;
            }

            return new TaylorBotEmbedResult(embed.Build());
        }

        [Command("claim")]
        [Summary("Claims the taypoints from all inactive users that added you to their taypoint will.")]
        public async Task<RuntimeResult> ClaimAsync()
        {
            var wills = await _taypointWillRepository.GetWillsWithBeneficiaryAsync(Context.User);

            var isInactive = wills.ToLookup(r => r.OwnerLatestSpokeAt < DateTimeOffset.Now.AddDays(-_options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed));
            var expiredWills = isInactive[true].ToList();

            var embed = new EmbedBuilder().WithUserAsAuthor(Context.User);

            if (expiredWills.Any())
            {
                var ownerUserIds = expiredWills.Select(r => r.OwnerUserId).ToList();
                var transfers = await _taypointWillRepository.TransferAllPointsAsync(ownerUserIds, Context.User);
                await _taypointWillRepository.RemoveWillsWithBeneficiaryAsync(ownerUserIds, Context.User);
                var transfersTo = transfers.ToLookup(t => t.UserId.Id == Context.User.Id);
                var receiver = transfersTo[true].Single();
                var gifters = transfersTo[false].ToList();
                var gainedPoints = receiver.TaypointCount - receiver.OriginalTaypointCount;
                embed
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join("\n",
                        new[] { $"Successfully claimed {"taypoint".ToQuantity(gainedPoints, TaylorBotFormats.BoldReadable)}, you now have {receiver.TaypointCount.ToString(TaylorBotFormats.Readable)}." }
                        .Concat(gifters.Select(g => $"Claimed {"taypoint".ToQuantity(g.OriginalTaypointCount, TaylorBotFormats.BoldReadable)} from {MentionUtils.MentionUser(g.UserId.Id)}."))
                    ).Truncate(2048));
            }
            else
            {
                var ongoingWills = isInactive[false].ToList();
                embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription(string.Join("\n",
                        new[] { $"None of the {"taypoint will".ToQuantity(wills.Count)} you are beneficiary of is ready to claim." }
                        .Concat(ongoingWills.Select(w => $"{MentionUtils.MentionUser(w.OwnerUserId.Id)} has been active on {w.OwnerLatestSpokeAt.FormatShortUserDate(TaylorBotCulture.Culture)}."))
                    ).Truncate(2048));
            }

            return new TaylorBotEmbedResult(embed.Build());
        }
    }
}
