using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.PageMessages;

public class PageCancelButtonHandler(
    InteractionResponseClient responseClient,
    IPageNavigationService navigationService) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.PageCancel;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        // Remove the page from navigation service
        await navigationService.RemovePageAsync(button.Message.id);

        // Update the message to indicate it was cancelled
        await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed("Page navigation cancelled."));
    }
}
