using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;

public class TaypointsSuccessionSlashCommand(
    ITaypointWillRepository taypointWillRepository,
    IOptionsMonitor<TaypointWillOptions> options) : ISlashCommand<NoOptions>
{
    public static string CommandName => "taypoints succession";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public async ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new Command(
            new(Info.Name),
            async () =>
            {
                // Get the user's current will
                var user = context.User;
                var will = await taypointWillRepository.GetWillAsync(owner: user);

                // Get wills the user is beneficiary of
                var willsAsBeneficiary = await taypointWillRepository.GetWillsWithBeneficiaryAsync(user);

                // Number of days of inactivity required
                var daysRequired = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;

                // Check if there are any wills the user can claim
                var expiredWills = willsAsBeneficiary
                    .Where(w => w.OwnerLatestSpokeAt < DateTimeOffset.UtcNow.AddDays(-daysRequired))
                    .ToList();

                // Build the description based on current status
                var description = BuildWillStatusDescription(user, will, willsAsBeneficiary, daysRequired, expiredWills);

                var embed = EmbedFactory.CreateSuccess(description);

                // Create action components based on status
                List<InteractionResponse.Component> components = [];

                // Add button or select menu components based on will status
                if (will == null)
                {
                    // User select menu to add a beneficiary
                    components.Add(InteractionResponse.Component.CreateUserSelect(
                        custom_id: "taypoints-succession:add-beneficiary-select",
                        placeholder: "Select a beneficiary for your will",
                        min_values: 1,
                        max_values: 1));
                }
                else
                {
                    // Show a delete button
                    components.Add(InteractionResponse.Component.CreateButton(
                        style: InteractionButtonStyle.Danger,
                        custom_id: "taypoints-succession:remove-beneficiary",
                        label: "Remove Beneficiary"));

                    // Show user select with current beneficiary preselected
                    components.Add(InteractionResponse.Component.CreateUserSelect(
                        custom_id: "taypoints-succession:change-beneficiary-select",
                        placeholder: "Change your will beneficiary",
                        default_values: [new($"{will.BeneficiaryUserId}", "user")],
                        min_values: 1,
                        max_values: 1));
                }

                // Add claim button if there are claimable wills
                if (expiredWills.Count > 0)
                {
                    components.Add(InteractionResponse.Component.CreateActionRow([InteractionResponse.Component.CreateButton(
                        style: InteractionButtonStyle.Primary,
                        custom_id: "taypoints-succession:claim-taypoints",
                        label: $"Claim Taypoints ({expiredWills.Count})")]));
                }

                return new MessageResult(new(new(embed), components));
            }
        );
    }

    private string BuildWillStatusDescription(
        DiscordUser user,
        Will? will,
        IReadOnlyCollection<WillOwner> willsAsBeneficiary,
        uint daysRequired,
        IReadOnlyCollection<WillOwner> expiredWills)
    {
        var description = new List<string>();

        // Current will status
        if (will != null)
        {
            description.Add($"__**Your Will**__");
            description.Add($"Your taypoint will has a beneficiary: {will.BeneficiaryUsername} (<@{will.BeneficiaryUserId}>).");
            description.Add($"If you are inactive for {"day".ToQuantity(daysRequired)} in all servers I'm in, they can claim all your taypoints.");
        }
        else
        {
            description.Add($"__**Your Will**__");
            description.Add($"You haven't set up a taypoint will yet. If you become inactive, your taypoints won't be used!");
            description.Add($"Add a beneficiary to ensure your taypoints aren't lost if you're inactive.");
        }

        // Add wills where user is beneficiary
        if (willsAsBeneficiary.Count > 0)
        {
            description.Add("");
            description.Add($"__**Wills You're Beneficiary Of**__");

            if (expiredWills.Count > 0)
            {
                description.Add($"You can claim taypoints from {"user".ToQuantity(expiredWills.Count)} due to inactivity:");
                foreach (var expiredWill in expiredWills.Take(3))
                {
                    description.Add($"• {expiredWill.OwnerUsername} (<@{expiredWill.OwnerUserId.Id}>) - Last active: {expiredWill.OwnerLatestSpokeAt.FormatShortUserDate(TaylorBotCulture.Culture)}");
                }

                if (expiredWills.Count > 3)
                {
                    description.Add($"• ...and {expiredWills.Count - 3} more");
                }
            }

            var activeWills = willsAsBeneficiary.Except(expiredWills).ToList();
            if (activeWills.Count > 0)
            {
                if (expiredWills.Count > 0)
                {
                    description.Add("");
                }

                description.Add($"You're beneficiary of {"will".ToQuantity(activeWills.Count)} from active users:");
                foreach (var activeWill in activeWills.Take(3))
                {
                    description.Add($"• {activeWill.OwnerUsername} (<@{activeWill.OwnerUserId.Id}>) - Last active: {activeWill.OwnerLatestSpokeAt.FormatShortUserDate(TaylorBotCulture.Culture)}");
                }

                if (activeWills.Count > 3)
                {
                    description.Add($"• ...and {activeWills.Count - 3} more");
                }
            }
        }

        return string.Join("\n", description);
    }
}
