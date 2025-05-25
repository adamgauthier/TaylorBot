using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;

public class TaypointsSuccessionClearSuccessorHandler(
    ITaypointWillRepository taypointWillRepository,
    IInteractionResponseClient responseClient) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.TaypointsSuccessionClearSuccessor;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        await taypointWillRepository.RemoveWillWithOwnerAsync(context.User);

        var embed = EmbedFactory.CreateSuccess(
            $"""
            Your taypoint successor has been cleared ✅
            **Your taypoints are NOT safe**, secure your taypoints now by choosing a trusted successor ⚠️
            """); ;

        await responseClient.EditOriginalResponseAsync(button.Interaction, new MessageResponse(new([embed]), [TaypointsSuccessionSlashCommand.CreateSuccessorUserSelect()]));
    }
}

public class TaypointsSuccessionChangeSuccessorHandler(
    ITaypointWillRepository taypointWillRepository,
    IInteractionResponseClient responseClient) : IUserSelectHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.TaypointsSuccessionChangeSuccessor;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordUserSelectComponent userSelect, RunContext context)
    {
        var selectedUser = userSelect.SelectedUsers.Single();

        if (selectedUser.Id == context.User.Id)
        {
            await responseClient.EditOriginalResponseAsync(userSelect.Interaction, EmbedFactory.CreateErrorEmbed(
                """
                You can't add yourself as successor 😕
                Pick someone you trust! 🤝
                """));

            return;
        }

        await taypointWillRepository.AddWillAsync(owner: context.User, beneficiary: selectedUser);

        var embed = EmbedFactory.CreateSuccess(
            $"""
            Your taypoint successor has been set to {selectedUser.Username} ({MentionUtils.MentionUser(selectedUser.Id)}) ✅
            **Your taypoints are safe** and can be claimed if you become inactive 🔑
            """);

        var successorSelect = TaypointsSuccessionSlashCommand.CreateSuccessorUserSelect(selectedUser.Id);
        var removeButton = TaypointsSuccessionSlashCommand.CreateClearSuccessorButton();

        await responseClient.EditOriginalResponseAsync(userSelect.Interaction, new MessageResponse(new([embed]), [successorSelect, removeButton]));
    }
}
