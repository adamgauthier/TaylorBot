using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.PostExecution;

public class GenericPromptCancelButtonHandler(InteractionResponseClient responseClient) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.GenericPromptCancel;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText());

    public async Task HandleAsync(DiscordButtonComponent button)
    {
        await responseClient.EditOriginalResponseAsync(button, EmbedFactory.CreateErrorEmbed("Operation cancelled 👍"));
    }
}
