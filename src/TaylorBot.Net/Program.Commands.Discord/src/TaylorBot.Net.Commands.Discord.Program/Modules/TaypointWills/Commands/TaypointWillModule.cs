using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;

[Name("TaypointWill")]
[Group("taypointwill")]
public class TaypointWillModule(ICommandRunner commandRunner, IOptionsMonitor<TaypointWillOptions> options, ITaypointWillRepository taypointWillRepository) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    [Summary("Displays a user's taypoint will.")]
    public async Task<RuntimeResult> GetAsync(
        [Summary("What user would you like to see the taypoint will of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
        {
            var u = user == null ? Context.User : await user.GetTrackedUserAsync();

            var will = await taypointWillRepository.GetWillAsync(owner: u);

            var embed = new EmbedBuilder().WithUserAsAuthor(u);

            if (will != null)
            {
                var days = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;
                var beneficiary = will.BeneficiaryUsername;
                embed
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"{u.Username}'s taypoint will has a beneficiary: {beneficiary} ({MentionUtils.MentionUser(will.BeneficiaryUserId.Id)}).",
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

            return new EmbedResult(embed.Build());
        });

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("add")]
    [Summary("Adds a user as beneficiary to your taypoint will.")]
    public async Task<RuntimeResult> AddAsync(
        [Summary("What user would you like to add to your taypoint will?")]
        [Remainder]
        IMentionedUserNotAuthor<IUser> mentionedUser
    )
    {
        var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
        {
            var user = await mentionedUser.GetTrackedUserAsync();

            var result = await taypointWillRepository.AddWillAsync(owner: Context.User, beneficiary: user);

            var embed = new EmbedBuilder().WithUserAsAuthor(Context.User);

            switch (result)
            {
                case WillAddedResult _:
                    var days = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;
                    var prefix = Context.CommandPrefix;
                    embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Successfully added {user.Username} ({user.Mention}) as beneficiary to your taypoint will.",
                            $"If you are inactive for {"day".ToQuantity(days)} in all servers I'm in, {user.Username} can claim **all your taypoints** with `{prefix}taypointwill claim`.",
                            $"If you change your mind at any time, you can use `{prefix}taypointwill clear` to remove them."
                        }));
                    break;
                case WillNotAddedResult willNotAdded:
                    var formattedBeneficiary = $"{willNotAdded.CurrentBeneficiaryUsername} ({MentionUtils.MentionUser(willNotAdded.CurrentBeneficiaryId.Id)})";
                    embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Can't add {user.Username} ({user.Mention}) to your taypoint will because it is set to {formattedBeneficiary}.",
                            $"If you want to change your beneficiary, you first need to use `{Context.CommandPrefix}taypointwill clear`.",
                        }));
                    break;
            }

            return new EmbedResult(embed.Build());
        });

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("clear")]
    [Summary("Clears your taypoint will.")]
    public async Task<RuntimeResult> ClearAsync()
    {
        var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
        {
            var result = await taypointWillRepository.RemoveWillWithOwnerAsync(Context.User);

            var embed = new EmbedBuilder().WithUserAsAuthor(Context.User);

            switch (result)
            {
                case WillRemovedResult willRemoved:
                    var formattedBeneficiary = $"{willRemoved.RemovedBeneficiaryUsername} ({MentionUtils.MentionUser(willRemoved.RemovedBeneficiaryId.Id)})";
                    embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                        $"Your taypoint will with {formattedBeneficiary} has been cleared.",
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

            return new EmbedResult(embed.Build());
        });

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("claim")]
    [Summary("Claims the taypoints from all inactive users that added you to their taypoint will.")]
    public async Task<RuntimeResult> ClaimAsync()
    {
        var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
        {
            var wills = await taypointWillRepository.GetWillsWithBeneficiaryAsync(Context.User);

            var isInactive = wills.ToLookup(r => r.OwnerLatestSpokeAt < DateTimeOffset.Now.AddDays(-options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed));
            var expiredWills = isInactive[true].ToList();

            var embed = new EmbedBuilder().WithUserAsAuthor(Context.User);

            if (expiredWills.Any())
            {
                var ownerUserIds = expiredWills.Select(r => r.OwnerUserId).ToList();
                var transfers = await taypointWillRepository.TransferAllPointsAsync(ownerUserIds, Context.User);
                await taypointWillRepository.RemoveWillsWithBeneficiaryAsync(ownerUserIds, Context.User);
                var transfersTo = transfers.ToLookup(t => t.UserId.Id == Context.User.Id);
                var receiver = transfersTo[true].Single();
                var gifters = transfersTo[false].ToList();
                var gainedPoints = receiver.TaypointCount - receiver.OriginalTaypointCount;

                string FormatTaypointQuantity(long taypointCount) => "taypoint".ToQuantity(gainedPoints, TaylorBotFormats.BoldReadable);

                embed
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join("\n",
                        new[] { $"Successfully claimed {FormatTaypointQuantity(gainedPoints)}, you now have {receiver.TaypointCount.ToString(TaylorBotFormats.Readable)}." }
                        .Concat(gifters.Select(g =>
                            $"Claimed {FormatTaypointQuantity(g.OriginalTaypointCount)} from {g.Username} ({MentionUtils.MentionUser(g.UserId.Id)})."
                        ))
                    ).Truncate(EmbedBuilder.MaxDescriptionLength));
            }
            else
            {
                var ongoingWills = isInactive[false].ToList();
                embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription(string.Join("\n",
                        new[] { $"None of the {"taypoint will".ToQuantity(wills.Count)} you are beneficiary of is ready to claim." }
                        .Concat(ongoingWills.Select(w =>
                            $"{w.OwnerUsername} ({MentionUtils.MentionUser(w.OwnerUserId.Id)}) was active on {w.OwnerLatestSpokeAt.FormatShortUserDate(TaylorBotCulture.Culture)}."
                        ))
                    ).Truncate(EmbedBuilder.MaxDescriptionLength));
            }

            return new EmbedResult(embed.Build());
        });

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
