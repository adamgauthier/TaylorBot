namespace TaylorBot.Net.Commands.PostExecution;

public class GenericMessageDeleteButtonHandler(InteractionResponseClient responseClient) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.GenericMessageDelete;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        await responseClient.DeleteOriginalResponseAsync(button);
    }
}
