using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.PostExecution;

public record ButtonComponent(
    string Id,
    string Token,
    string CustomId,
    string MessageId,
    string UserId
) : IInteraction;

public class MessageComponentHandler
{
    private readonly ILogger<MessageComponentHandler> _logger;
    private readonly InteractionResponseClient _interactionResponseClient;

    private readonly Dictionary<string, Func<ButtonComponent, ValueTask>> _callbacks = new();

    public MessageComponentHandler(ILogger<MessageComponentHandler> logger, InteractionResponseClient interactionResponseClient)
    {
        _logger = logger;
        _interactionResponseClient = interactionResponseClient;
    }

    public async ValueTask HandleAsync(Interaction interaction)
    {
        switch (interaction.data!.component_type!)
        {
            case 2:
                ButtonComponent button = new(
                    interaction.id,
                    interaction.token,
                    interaction.data!.custom_id!,
                    interaction.message!.id,
                    interaction.user != null ? interaction.user.id : interaction.member!.user.id
                );

                if (_callbacks.TryGetValue(button.CustomId, out var callback))
                {
                    await _interactionResponseClient.SendComponentAckResponseWithoutLoadingMessageAsync(button);

                    await callback(button);
                }
                else
                {
                    _logger.LogWarning("Button component without callback: {Interaction}", interaction);
                }
                break;

            default:
                _logger.LogWarning("Unknown component type: {Interaction}", interaction);
                break;
        }
    }

    public void AddCallback(string customId, Func<ButtonComponent, ValueTask> callback)
    {
        _callbacks.Add(customId, callback);
    }

    public void RemoveCallback(string customId)
    {
        _callbacks.Remove(customId);
    }
}
