using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Client;
using static TaylorBot.Net.Commands.PostExecution.ModalSubmit;

namespace TaylorBot.Net.Commands.PostExecution;

public record ModalSubmit(
    string Id,
    string Token,
    string CustomId,
    string UserId,
    string? GuildId,
    IReadOnlyList<TextInputSubmit> TextInputs
) : IInteraction
{
    public record TextInputSubmit(string CustomId, string Value);
};

public record ModalCallback(Func<ModalSubmit, ValueTask> SubmitAsync, bool IsPrivateResponse);

public class ModalInteractionHandler(ILogger<ModalInteractionHandler> logger, InteractionResponseClient interactionResponseClient)
{
    private readonly Dictionary<string, ModalCallback> _callbacks = [];

    public async ValueTask HandleAsync(Interaction interaction)
    {
        ModalSubmit submit = new(
            interaction.id,
            interaction.token,
            interaction.data!.custom_id!,
            interaction.user != null ? interaction.user.id : interaction.member!.user.id,
            interaction.guild_id,
            interaction.data!.components!.Select(c => c.components![0]).Select(c => new TextInputSubmit(c.custom_id!, c.value!)).ToList()
        );

        if (_callbacks.TryGetValue(submit.CustomId, out var callback))
        {
            await interactionResponseClient.SendAckResponseWithLoadingMessageAsync(submit, callback.IsPrivateResponse);

            await callback.SubmitAsync(submit);
        }
        else
        {
            logger.LogWarning("Modal create without callback: {Interaction}", interaction);
        }
    }

    public void AddCallback(string customId, ModalCallback callback)
    {
        _callbacks.Add(customId, callback);
    }

    public void RemoveCallback(string customId)
    {
        _callbacks.Remove(customId);
    }
}
