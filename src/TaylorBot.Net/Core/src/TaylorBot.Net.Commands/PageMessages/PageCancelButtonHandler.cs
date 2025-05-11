using TaylorBot.Net.Commands.PostExecution;

namespace TaylorBot.Net.Commands.PageMessages;

public class PageCancelButtonHandler(
    InteractionResponseClient responseClient,
    PageOptionsInMemoryRepository pageOptionsRepository) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.PageMessageCancel;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var optionsId = Guid.ParseExact(button.CustomId.ParsedData["opt"], "N");

        pageOptionsRepository.Remove(optionsId);

        await responseClient.DeleteOriginalResponseAsync(button);
    }
}
