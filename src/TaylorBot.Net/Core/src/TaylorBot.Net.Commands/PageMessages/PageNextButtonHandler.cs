using TaylorBot.Net.Commands.PostExecution;
using static TaylorBot.Net.Commands.PostExecution.InteractionResponseClient;

namespace TaylorBot.Net.Commands.PageMessages;

public class PageNextButtonHandler(InteractionResponseClient responseClient, PageOptionsInMemoryRepository pageOptionsRepository, PageMessageFactory pageMessageFactory) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.PageMessageNext;

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
                await responseClient.PatchComponentsAsync(button.Interaction, ToInteractionComponents([cancelButton.Button]));
            }
            else
            {
                await responseClient.PatchComponentsAsync(button.Interaction, []);
            }
            return;
        }

        cached.Renderer.RenderNext();
        var pageMessage = pageMessageFactory.Create(cached);
        ArgumentNullException.ThrowIfNull(pageMessage.Buttons);

        await responseClient.EditOriginalResponseAsync(button.Interaction, message: new(pageMessage.Content, [.. pageMessage.Buttons.Buttons.Select(b => b.Button)]));
    }
}
