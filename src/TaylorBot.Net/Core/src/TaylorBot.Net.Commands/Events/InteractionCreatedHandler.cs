using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Events
{
    public class InteractionCreatedHandler : IInteractionCreatedHandler
    {
        private const int APPLICATION_COMMAND = 2;
        private const int MESSAGE_COMPONENT = 3;
        private const int MODAL_SUBMIT = 5;

        private readonly ILogger<InteractionCreatedHandler> _logger;
        private readonly SlashCommandHandler _slashCommandHandler;
        private readonly MessageComponentHandler _messageComponentHandler;
        private readonly ModalInteractionHandler _modalInteractionHandler;
        private readonly TaskExceptionLogger _taskExceptionLogger;

        public InteractionCreatedHandler(
            ILogger<InteractionCreatedHandler> logger,
            SlashCommandHandler slashCommandHandler,
            MessageComponentHandler messageComponentHandler,
            ModalInteractionHandler modalInteractionHandler,
            TaskExceptionLogger taskExceptionLogger)
        {
            _logger = logger;
            _slashCommandHandler = slashCommandHandler;
            _messageComponentHandler = messageComponentHandler;
            _modalInteractionHandler = modalInteractionHandler;
            _taskExceptionLogger = taskExceptionLogger;
        }

        public Task InteractionCreatedAsync(Interaction interaction)
        {
            switch (interaction.type)
            {
                case APPLICATION_COMMAND:
                    _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                        async () => await _slashCommandHandler.HandleAsync(interaction),
                        nameof(SlashCommandHandler)
                    ));
                    break;

                case MESSAGE_COMPONENT:
                    _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                        async () => await _messageComponentHandler.HandleAsync(interaction),
                        nameof(MessageComponentHandler)
                    ));
                    break;

                case MODAL_SUBMIT:
                    _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                        async () => await _modalInteractionHandler.HandleAsync(interaction),
                        nameof(MessageComponentHandler)
                    ));
                    break;

                default:
                    _logger.LogWarning("Unknown interaction type: {Interaction}", interaction);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
