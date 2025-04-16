using TaylorBot.Net.Commands.PostExecution;

namespace TaylorBot.Net.Commands.PageMessages;

public class PagePreviousButtonHandler(InteractionResponseClient responseClient, PageOptionsInMemoryRepository pageOptionsRepository, PageMessageFactory pageMessageFactory) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.PageMessagePrevious;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var optionsId = Guid.ParseExact(button.CustomId.ParsedData["opt"], "N");
        var cached = pageOptionsRepository.Get(optionsId);
        if (cached == null)
        {
            // remove buttons?
            return;
        }

        cached.Renderer.RenderPrevious();
        var pageMessage = pageMessageFactory.Create(cached);
        ArgumentNullException.ThrowIfNull(pageMessage.Buttons);

        await responseClient.EditOriginalResponseAsync(button.Interaction, message: new(pageMessage.Content, [.. pageMessage.Buttons.Buttons.Select(b => b.Button)]));
    }
}
