using TaylorBot.Net.Commands.PostExecution;
using static TaylorBot.Net.Commands.PostExecution.InteractionResponseClient;

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
            // Keep only the cancel button if it exists
            var hasCancelButton = button.Message.components?
                .SelectMany(c => c.components ?? [])
                .Any(c => c.custom_id is not null && new InteractionCustomId(c.custom_id).ParsedName == CustomIdNames.PageMessageCancel) == true;

            if (hasCancelButton)
            {
                var cancelButton = PageMessageFactory.CreateCancelButton(button.CustomId.DataEntries);
                await responseClient.PatchComponentsAsync(button.Interaction, ToInteractionComponents([cancelButton]));
            }
            else
            {
                await responseClient.PatchComponentsAsync(button.Interaction, []);
            }
            return;
        }

        cached.Renderer.RenderPrevious();
        var pageMessage = pageMessageFactory.Create(cached);
        ArgumentNullException.ThrowIfNull(pageMessage.Message.Buttons);

        await responseClient.EditOriginalResponseAsync(button.Interaction, message: pageMessage.Message);
    }
}
