using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;

public class TaypointsSuccessionClaimTaypointsHandler(
   TimeProvider timeProvider,
   CommandMentioner mention,
   ITaypointWillRepository taypointWillRepository,
   IOptionsMonitor<TaypointWillOptions> options,
   IInteractionResponseClient responseClient) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.TaypointsSuccessionClaim;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var willsAsBeneficiary = await taypointWillRepository.GetWillsWithBeneficiaryAsync(context.User);
        var daysRequired = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;

        var expiredWills = willsAsBeneficiary
            .Where(w => w.OwnerLatestSpokeAt < timeProvider.GetUtcNow().AddDays(-daysRequired))
            .ToList();

        if (expiredWills.Count != 0)
        {
            var ownerUserIds = expiredWills.Select(r => r.OwnerUserId).ToList();
            var transfers = await taypointWillRepository.TransferAllPointsAsync(ownerUserIds, context.User);
            await taypointWillRepository.RemoveWillsWithBeneficiaryAsync(ownerUserIds, context.User);

            var transfersTo = transfers.ToLookup(t => t.UserId.Id == context.User.Id);
            var receiver = transfersTo[true].Single();
            var gifters = transfersTo[false].ToList();
            var gainedPoints = receiver.TaypointCount - receiver.OriginalTaypointCount;

            static string FormatTaypointQuantity(long taypointCount) => "taypoint".ToQuantity(taypointCount, TaylorBotFormats.BoldReadable, TaylorBotCulture.Culture);

            var embed = EmbedFactory.CreateSuccessEmbed(
                $"""
                Successfully claimed {FormatTaypointQuantity(gainedPoints)}, you now have {receiver.TaypointCount.ToString(TaylorBotFormats.Readable, TaylorBotCulture.Culture)} 💰
                {string.Join("\n", gifters.Select(g =>
                    $"- Claimed {FormatTaypointQuantity(g.OriginalTaypointCount)} from {g.Username} ({MentionUtils.MentionUser(g.UserId)}) 🤝"
                ))}
                """.Truncate(EmbedBuilder.MaxDescriptionLength));

            await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
        }
        else
        {
            var ongoingSuccessions = willsAsBeneficiary.Where(w => w.OwnerLatestSpokeAt >= timeProvider.GetUtcNow().AddDays(-daysRequired)).ToList();

            var embed = EmbedFactory.CreateErrorEmbed(
                $"""
                None of the {"taypoint succession".ToQuantity(willsAsBeneficiary.Count)} you are successor of is ready to claim 🤔
                {TaypointsSuccessionSlashCommand.FormatWillOwnersList(
                    [.. ongoingSuccessions.OrderBy(w => w.OwnerLatestSpokeAt)],
                    maxDisplayed: 5
                )}
                Use {mention.SlashCommand("taypoints succession", context)} to manage your succession 🔒
                """.Truncate(EmbedBuilder.MaxDescriptionLength));

            await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
        }
    }
}

public class TaypointsSuccessionClaimSkipHandler(
   TaypointsSuccessionSlashCommand taypointsSuccessionCommand,
   IInteractionResponseClient responseClient) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.TaypointsSuccessionClaimSkip;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var response = await taypointsSuccessionCommand.GetManageSuccessionResponseAsync(context.User);
        await responseClient.EditOriginalResponseAsync(button.Interaction, response);
    }
}
