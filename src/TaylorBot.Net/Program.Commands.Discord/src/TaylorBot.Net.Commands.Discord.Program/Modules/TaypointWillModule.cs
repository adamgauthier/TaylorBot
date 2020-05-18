using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
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

        [Command("claim")]
        [Summary("Claims the taypoints from all inactive users that have you on their taypoint will.")]
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
                await _taypointWillRepository.RemoveWillsAsync(ownerUserIds, Context.User);
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
                        new[] { $"None of the {"taypoint will".ToQuantity(wills.Count)} mentioning you is ready to claim." }
                        .Concat(ongoingWills.Select(w => $"{MentionUtils.MentionUser(w.OwnerUserId.Id)} has been active on {w.OwnerLatestSpokeAt.FormatShortUserDate(TaylorBotCulture.Culture)}."))
                    ).Truncate(2048));
            }

            return new TaylorBotEmbedResult(embed.Build());
        }
    }
}
