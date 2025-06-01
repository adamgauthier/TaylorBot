using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;

public class TaypointsSuccessionSlashCommand(
    ITaypointWillRepository taypointWillRepository,
    IOptionsMonitor<TaypointWillOptions> options,
    TimeProvider timeProvider) : ISlashCommand<NoOptions>
{
    public static string CommandName => "taypoints succession";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = context.User;

                var willsAsBeneficiary = await taypointWillRepository.GetWillsWithBeneficiaryAsync(user);

                var daysRequired = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;

                var expiredWills = willsAsBeneficiary
                    .Where(w => w.OwnerLatestSpokeAt < timeProvider.GetUtcNow().AddDays(-daysRequired))
                    .ToList();

                if (expiredWills.Count > 0)
                {
                    return GetClaimSuccessionResponse(user, expiredWills, daysRequired);
                }
                else
                {
                    return new MessageResult(await GetManageSuccessionResponseAsync(user, willsAsBeneficiary, daysRequired));
                }
            }
        ));
    }

    public static string FormatWillOwnersList(
        IReadOnlyCollection<WillOwner> wills,
        int maxDisplayed = 3)
    {
        var lines = wills.Take(maxDisplayed).Select(will =>
            $"- {will.OwnerUsername} {MentionUtils.MentionUser(will.OwnerUserId)}, last active: {will.OwnerLatestSpokeAt.FormatShortUserDate(TaylorBotCulture.Culture)}");

        if (wills.Count > maxDisplayed)
        {
            lines = lines.Append($"- ...and {wills.Count - maxDisplayed} more");
        }

        return string.Join("\n", lines);
    }

    private async Task<MessageResponse> GetManageSuccessionResponseAsync(
        DiscordUser user,
        IReadOnlyCollection<WillOwner> willsAsBeneficiary,
        uint daysRequired)
    {
        var will = await taypointWillRepository.GetWillAsync(owner: user);

        List<string> description = [
            $"""
            ## Taypoints Succession 🤝
            Succession allows you to secure your taypoints if your account become inactive (forgot password, hacked, locked by Discord, etc.) 🪙
            You can choose a trusted successor that will be able to claim your taypoints if you become inactive for {"day".ToQuantity(daysRequired)} 🔑
            """
        ];

        if (willsAsBeneficiary.Count > 0)
        {
            description.Add(
                $"""
                ## Trusting You as Successor 🫂
                You're the trusted successor for these users:
                {FormatWillOwnersList([.. willsAsBeneficiary.OrderBy(w => w.OwnerLatestSpokeAt)])}
                """);
        }

        description.Add(
            $"""
            ## Your Successor 💖
            {(will is not null
                ? $"**Your taypoints are safe** and will be transferred to {will.BeneficiaryUsername} ({MentionUtils.MentionUser(will.BeneficiaryUserId)}) if you become inactive for {"day".ToQuantity(daysRequired)} 🔒"
                : "**Your taypoints are NOT safe**, secure your taypoints now by choosing a trusted successor ⚠️")}
            """);

        var embed = EmbedFactory.CreateSuccess(string.Join("\n", description));

        List<InteractionComponent> components =
        [
            CreateSuccessorUserSelect(will?.BeneficiaryUserId)
        ];

        if (will != null)
        {
            components.Add(CreateClearSuccessorButton());
        }

        return new(new(embed), components);
    }

    public async Task<MessageResponse> GetManageSuccessionResponseAsync(DiscordUser user)
    {
        var willsAsBeneficiary = await taypointWillRepository.GetWillsWithBeneficiaryAsync(user);
        var daysRequired = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;
        return await GetManageSuccessionResponseAsync(user, willsAsBeneficiary, daysRequired);
    }

    public static InteractionComponent CreateSuccessorUserSelect(SnowflakeId? defaultUserId = null)
    {
        return InteractionComponent.CreateUserSelect(
            custom_id: InteractionCustomId.Create(CustomIdNames.TaypointsSuccessionChangeSuccessor).RawId,
            placeholder: "Pick a trusted taypoint successor",
            default_values: defaultUserId != null ? [new($"{defaultUserId}", "user")] : null,
            min_values: 1,
            max_values: 1);
    }

    public static InteractionComponent CreateClearSuccessorButton()
    {
        return InteractionComponent.CreateActionRow(InteractionComponent.CreateButton(
            style: InteractionButtonStyle.Danger,
            custom_id: InteractionCustomId.Create(CustomIdNames.TaypointsSuccessionClearSuccessor).RawId,
            label: "Remove Successor",
            emoji: new("🗑️")));
    }

    private MessageResult GetClaimSuccessionResponse(
        DiscordUser user,
        IReadOnlyCollection<WillOwner> expiredWills,
        uint daysRequired)
    {
        var embed = EmbedFactory.CreateSuccess(
            $"""
            ## Taypoints Available to Claim 🪙
            You can claim taypoints from {"user".ToQuantity(expiredWills.Count)} who have been inactive for more than {"day".ToQuantity(daysRequired)}:
            {FormatWillOwnersList([.. expiredWills.OrderBy(w => w.OwnerLatestSpokeAt)])}
            """);

        var claimButton = InteractionComponent.CreateButton(
            style: InteractionButtonStyle.Success,
            custom_id: InteractionCustomId.Create(CustomIdNames.TaypointsSuccessionClaim).RawId,
            label: "Claim");

        var skipButton = InteractionComponent.CreateButton(
            style: InteractionButtonStyle.Danger,
            custom_id: InteractionCustomId.Create(CustomIdNames.TaypointsSuccessionClaimSkip).RawId,
            label: "Skip");

        return new MessageResult(new(new(embed), [InteractionComponent.CreateActionRow(claimButton, skipButton)]));
    }
}
