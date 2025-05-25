using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.PostExecution;

public class GenericPromptCancelButtonHandler(IInteractionResponseClient responseClient) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.GenericPromptCancel;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed("Operation cancelled 👍"));
    }
}
