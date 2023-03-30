using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static TaylorBot.Net.Commands.PostExecution.ModalSubmit;

namespace TaylorBot.Net.Commands.PostExecution
{
    public record ModalSubmit(
        string Id,
        string Token,
        string CustomId,
        string UserId,
        string? GuildId,
        IReadOnlyList<TextInputSubmit> TextInputs
    )
    {
        public record TextInputSubmit(string CustomId, string Value);
    };

    public record ModalCallback(Func<ModalSubmit, ValueTask> SubmitAsync, bool IsPrivateResponse);

    public class ModalInteractionHandler
    {
        private readonly ILogger<ModalInteractionHandler> _logger;
        private readonly InteractionResponseClient _interactionResponseClient;

        private readonly Dictionary<string, ModalCallback> _callbacks = new();

        public ModalInteractionHandler(ILogger<ModalInteractionHandler> logger, InteractionResponseClient interactionResponseClient)
        {
            _logger = logger;
            _interactionResponseClient = interactionResponseClient;
        }

        public async ValueTask HandleAsync(Interaction interaction)
        {
            ModalSubmit submit = new(
                interaction.id,
                interaction.token,
                interaction.data!.custom_id!,
                interaction.user != null ? interaction.user.id : interaction.member!.user.id,
                interaction.guild_id,
                interaction.data!.components![0].components!.Select(c => new TextInputSubmit(c.custom_id!, c.value!)).ToList()
            );

            if (_callbacks.TryGetValue(submit.CustomId, out var callback))
            {
                await _interactionResponseClient.SendAckResponseWithLoadingMessageAsync(submit, callback.IsPrivateResponse);

                await callback.SubmitAsync(submit);
            }
            else
            {
                _logger.LogWarning("Modal create without callback: {Interaction}", interaction);
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
}
