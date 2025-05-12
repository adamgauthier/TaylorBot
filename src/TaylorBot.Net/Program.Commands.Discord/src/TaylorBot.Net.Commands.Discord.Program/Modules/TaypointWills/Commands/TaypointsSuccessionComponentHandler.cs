//using Discord;
//using Humanizer;
//using Microsoft.Extensions.Options;
//using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
//using TaylorBot.Net.Commands.Discord.Program.Options;
//using TaylorBot.Net.Commands.PostExecution;
//using TaylorBot.Net.Core.Embed;
//using TaylorBot.Net.Core.Number;
//using TaylorBot.Net.Core.Snowflake;
//using TaylorBot.Net.Core.User;

//namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;

//public class TaypointsSuccessionRemoveBeneficiaryHandler(
//    ITaypointWillRepository taypointWillRepository,
//    InteractionResponseClient responseClient) : IButtonHandler
//{
//    public static CustomIdNames CustomIdName => new CustomIdNames("taypoints-succession:remove-beneficiary");

//    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

//    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
//    {
//        var result = await taypointWillRepository.RemoveWillWithOwnerAsync(context.User);

//        var embed = EmbedFactory.CreateSuccess("Taypoints Succession Center");

//        switch (result)
//        {
//            case WillRemovedResult willRemoved:
//                var formattedBeneficiary = $"{willRemoved.RemovedBeneficiaryUsername} (<@{willRemoved.RemovedBeneficiaryId.Id}>)";
//                embed.WithDescription($"Your taypoint will with {formattedBeneficiary} has been cleared. You can add a new beneficiary with the select menu below.");

//                // Add the select menu to add a new beneficiary
//                var userSelect = InteractionResponse.Component.CreateUserSelect(
//                    custom_id: "taypoints-succession:add-beneficiary-select",
//                    placeholder: "Select a beneficiary for your will",
//                    min_values: 1,
//                    max_values: 1);

//                await responseClient.EditOriginalResponseAsync(button.Interaction, new(embed, [userSelect]));
//                break;

//            case WillNotRemovedResult _:
//                embed.WithDescription("You don't have a taypoint will to clear.");
//                await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
//                break;
//        }
//    }
//}

//public class TaypointsSuccessionAddBeneficiarySelectHandler(
//    ITaypointWillRepository taypointWillRepository,
//    IOptionsMonitor<TaypointWillOptions> options,
//    InteractionResponseClient responseClient) : IUserSelectComponentHandler
//{
//    public static CustomIdNames CustomIdName => new CustomIdNames("taypoints-succession:add-beneficiary-select");

//    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

//    public async Task HandleAsync(UserSelectComponent select, RunContext context)
//    {
//        // Get the selected user
//        var selectedUser = select.SelectedUsers.FirstOrDefault();
//        if (selectedUser == null)
//        {
//            await responseClient.EditOriginalResponseAsync(select.Interaction, EmbedFactory.CreateErrorEmbed("No user was selected."));
//            return;
//        }

//        // Check if the user selected themselves
//        if (selectedUser.Id == context.User.Id)
//        {
//            await responseClient.EditOriginalResponseAsync(select.Interaction, EmbedFactory.CreateErrorEmbed("You can't add yourself as a beneficiary."));
//            return;
//        }

//        // Add the beneficiary
//        var result = await taypointWillRepository.AddWillAsync(owner: context.User, beneficiary: selectedUser);

//        var embed = EmbedFactory.CreateSuccess("Taypoints Succession Center");

//        switch (result)
//        {
//            case WillAddedResult _:
//                var days = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;
//                embed.WithDescription(
//                    $"""
//                    Successfully added {selectedUser.Username} (<@{selectedUser.Id}>) as beneficiary to your taypoint will.
//                    If you are inactive for {"day".ToQuantity(days)} in all servers I'm in, they can claim **all your taypoints**.
//                    """);

//                // Add buttons for remove/change
//                var removeButton = InteractionResponse.Component.CreateButton(
//                    style: ButtonStyle.Danger,
//                    custom_id: "taypoints-succession:remove-beneficiary",
//                    label: "Remove Beneficiary");

//                var changeSelect = InteractionResponse.Component.CreateUserSelect(
//                    custom_id: "taypoints-succession:change-beneficiary-select",
//                    placeholder: "Change your will beneficiary",
//                    default_values: [selectedUser.Id.ToString()],
//                    min_values: 1,
//                    max_values: 1);

//                await responseClient.EditOriginalResponseAsync(select.Interaction, new(embed, [removeButton, changeSelect]));
//                break;

//            case WillNotAddedResult willNotAdded:
//                var formattedBeneficiary = $"{willNotAdded.CurrentBeneficiaryUsername} (<@{willNotAdded.CurrentBeneficiaryId.Id}>)";
//                embed.WithColor(TaylorBotColors.ErrorColor)
//                    .WithDescription(
//                        $"""
//                        Can't add {selectedUser.Username} (<@{selectedUser.Id}>) to your taypoint will because it is already set to {formattedBeneficiary}.
//                        Use the remove button to clear your current beneficiary first.
//                        """);

//                await responseClient.EditOriginalResponseAsync(select.Interaction, embed);
//                break;
//        }
//    }
//}

//public class TaypointsSuccessionChangeBeneficiarySelectHandler(
//    ITaypointWillRepository taypointWillRepository,
//    IOptionsMonitor<TaypointWillOptions> options,
//    InteractionResponseClient responseClient) : IUserSelectComponentHandler
//{
//    public static CustomIdNames CustomIdName => new CustomIdNames("taypoints-succession:change-beneficiary-select");

//    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

//    public async Task HandleAsync(UserSelectComponent select, RunContext context)
//    {
//        // Get the selected user
//        var selectedUser = select.SelectedUsers.FirstOrDefault();
//        if (selectedUser == null)
//        {
//            await responseClient.EditOriginalResponseAsync(select.Interaction, EmbedFactory.CreateErrorEmbed("No user was selected."));
//            return;
//        }

//        // Check if the user selected themselves
//        if (selectedUser.Id == context.User.Id)
//        {
//            await responseClient.EditOriginalResponseAsync(select.Interaction, EmbedFactory.CreateErrorEmbed("You can't add yourself as a beneficiary."));
//            return;
//        }

//        // Get current will
//        var currentWill = await taypointWillRepository.GetWillAsync(owner: context.User);
//        if (currentWill == null)
//        {
//            // This shouldn't happen normally, but handle it gracefully
//            var addResult = await taypointWillRepository.AddWillAsync(owner: context.User, beneficiary: selectedUser);
//            if (addResult is WillAddedResult)
//            {
//                var days = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;
//                var embed = EmbedFactory.CreateSuccess("Taypoints Succession Center")
//                    .WithDescription(
//                        $"""
//                        Successfully added {selectedUser.Username} (<@{selectedUser.Id}>) as beneficiary to your taypoint will.
//                        If you are inactive for {"day".ToQuantity(days)} in all servers I'm in, they can claim **all your taypoints**.
//                        """);

//                // Add buttons for remove/change
//                var removeButton = InteractionResponse.Component.CreateButton(
//                    style: ButtonStyle.Danger,
//                    custom_id: "taypoints-succession:remove-beneficiary",
//                    label: "Remove Beneficiary");

//                var changeSelect = InteractionResponse.Component.CreateUserSelect(
//                    custom_id: "taypoints-succession:change-beneficiary-select",
//                    placeholder: "Change your will beneficiary",
//                    default_values: [selectedUser.Id.ToString()],
//                    min_values: 1,
//                    max_values: 1);

//                await responseClient.EditOriginalResponseAsync(select.Interaction, new(embed, [removeButton, changeSelect]));
//            }
//            return;
//        }

//        // Is this the same beneficiary?
//        if (currentWill.BeneficiaryUserId.Id == selectedUser.Id)
//        {
//            await responseClient.EditOriginalResponseAsync(select.Interaction,
//                EmbedFactory.CreateInfoEmbed($"{selectedUser.Username} is already your beneficiary. No changes needed."));
//            return;
//        }

//        // Remove the old will and add the new one
//        await taypointWillRepository.RemoveWillWithOwnerAsync(context.User);
//        var result = await taypointWillRepository.AddWillAsync(owner: context.User, beneficiary: selectedUser);

//        if (result is WillAddedResult)
//        {
//            var days = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;
//            var embed = EmbedFactory.CreateSuccess("Taypoints Succession Center")
//                .WithDescription(
//                    $"""
//                    Successfully changed your beneficiary from {currentWill.BeneficiaryUsername} to {selectedUser.Username} (<@{selectedUser.Id}>).
//                    If you are inactive for {"day".ToQuantity(days)} in all servers I'm in, they can claim **all your taypoints**.
//                    """);

//            // Add buttons for remove/change
//            var removeButton = InteractionResponse.Component.CreateButton(
//                style: ButtonStyle.Danger,
//                custom_id: "taypoints-succession:remove-beneficiary",
//                label: "Remove Beneficiary");

//            var changeSelect = InteractionResponse.Component.CreateUserSelect(
//                custom_id: "taypoints-succession:change-beneficiary-select",
//                placeholder: "Change your will beneficiary",
//                default_values: [selectedUser.Id.ToString()],
//                min_values: 1,
//                max_values: 1);

//            await responseClient.EditOriginalResponseAsync(select.Interaction, new(embed, [removeButton, changeSelect]));
//        }
//    }
//}

//public class TaypointsSuccessionClaimTaypointsHandler(
//    ITaypointWillRepository taypointWillRepository,
//    IOptionsMonitor<TaypointWillOptions> options,
//    InteractionResponseClient responseClient) : IButtonHandler
//{
//    public static CustomIdNames CustomIdName => new CustomIdNames("taypoints-succession:claim-taypoints");

//    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

//    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
//    {
//        var wills = await taypointWillRepository.GetWillsWithBeneficiaryAsync(context.User);
//        var daysRequired = options.CurrentValue.DaysOfInactivityBeforeWillCanBeClaimed;

//        var expiredWills = wills
//            .Where(w => w.OwnerLatestSpokeAt < DateTimeOffset.UtcNow.AddDays(-daysRequired))
//            .ToList();

//        var embed = EmbedFactory.CreateSuccess("Taypoints Succession Center");

//        if (expiredWills.Count != 0)
//        {
//            var ownerUserIds = expiredWills.Select(r => r.OwnerUserId).ToList();
//            var transfers = await taypointWillRepository.TransferAllPointsAsync(ownerUserIds, context.User);
//            await taypointWillRepository.RemoveWillsWithBeneficiaryAsync(ownerUserIds, context.User);

//            var transfersTo = transfers.ToLookup(t => t.UserId.Id == context.User.Id);
//            var receiver = transfersTo[true].Single();
//            var gifters = transfersTo[false].ToList();
//            var gainedPoints = receiver.TaypointCount - receiver.OriginalTaypointCount;

//            string FormatTaypointQuantity(long taypointCount) => "taypoint".ToQuantity(taypointCount, TaylorBotFormats.BoldReadable);

//            embed.WithDescription(string.Join("\n",
//                new[] { $"Successfully claimed {FormatTaypointQuantity(gainedPoints)}, you now have {receiver.TaypointCount.ToString(TaylorBotFormats.Readable)}." }
//                .Concat(gifters.Select(g =>
//                    $"Claimed {FormatTaypointQuantity(g.OriginalTaypointCount)} from {g.Username} (<@{g.UserId.Id}>)."
//                ))
//            ).Truncate(EmbedBuilder.MaxDescriptionLength));

//            // Refresh the command UI
//            await GetRefreshedUIAsync(button, context);
//        }
//        else
//        {
//            var ongoingWills = wills.Where(w => w.OwnerLatestSpokeAt >= DateTimeOffset.UtcNow.AddDays(-daysRequired)).ToList();

//            embed.WithColor(TaylorBotColors.ErrorColor)
//                .WithDescription(string.Join("\n",
//                    new[] { $"None of the {"taypoint will".ToQuantity(wills.Count)} you are beneficiary of is ready to claim." }
//                    .Concat(ongoingWills.Take(5).Select(w =>
//                        $"• {w.OwnerUsername} (<@{w.OwnerUserId.Id}>) was active on {w.OwnerLatestSpokeAt.FormatShortUserDate(TaylorBotCulture.Culture)}."
//                    ))
//                    .Concat(ongoingWills.Count > 5 ? new[] { $"• ...and {ongoingWills.Count - 5} more" } : [])
//                ).Truncate(EmbedBuilder.MaxDescriptionLength));

//            await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
//        }
//    }

//    // Helper method to refresh the UI after claiming taypoints
//    private async Task GetRefreshedUIAsync(DiscordButtonComponent button, RunContext context)
//    {
//        // Trigger a refresh of the command by using the slash command
//        var slashCommand = new TaypointsSuccessionSlashCommand(taypointWillRepository, options);
//        var command = await slashCommand.GetCommandAsync(context, new NoOptions());
//        var result = await command.RunAsync();

//        if (result is MessageResult messageResult)
//        {
//            await responseClient.EditOriginalResponseAsync(button.Interaction, messageResult.Response);
//        }
//    }
//}
