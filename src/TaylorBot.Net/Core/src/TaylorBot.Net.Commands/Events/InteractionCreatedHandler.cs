using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Events
{
    public class InteractionCreatedHandler : IInteractionCreatedHandler
    {
        private readonly ILogger<InteractionCreatedHandler> _logger;
        private readonly SlashCommandHandler _slashCommandHandler;
        private readonly MessageComponentHandler _messageComponentHandler;
        private readonly TaskExceptionLogger _taskExceptionLogger;

        public InteractionCreatedHandler(ILogger<InteractionCreatedHandler> logger, SlashCommandHandler slashCommandHandler, MessageComponentHandler messageComponentHandler, TaskExceptionLogger taskExceptionLogger)
        {
            _logger = logger;
            _slashCommandHandler = slashCommandHandler;
            _messageComponentHandler = messageComponentHandler;
            _taskExceptionLogger = taskExceptionLogger;
        }

        public Task InteractionCreatedAsync(Interaction interaction)
        {
            switch (interaction.type)
            {
                case 2:
                    _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                        async () => await _slashCommandHandler.HandleAsync(interaction),
                        nameof(SlashCommandHandler)
                    ));
                    break;

                case 3:
                    _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                        async () => await _messageComponentHandler.HandleAsync(interaction),
                        nameof(MessageComponentHandler)
                    ));
                    break;

                default:
                    _logger.LogWarning($"Unknown interaction type: {interaction}");
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
