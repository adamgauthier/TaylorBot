using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Events;

public class InteractionCreatedHandler(
    ILogger<InteractionCreatedHandler> logger,
    CommandActivityFactory commandActivityFactory,
    SlashCommandHandler slashCommandHandler,
    MessageComponentHandler messageComponentHandler,
    ModalInteractionHandler modalInteractionHandler,
    TaskExceptionLogger taskExceptionLogger) : IInteractionCreatedHandler
{
    private const int APPLICATION_COMMAND = 2;
    private const int MESSAGE_COMPONENT = 3;
    private const int MODAL_SUBMIT = 5;

    public Task InteractionCreatedAsync(Interaction interaction)
    {
        switch (interaction.type)
        {
            case APPLICATION_COMMAND:
                _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                    async () =>
                    {
                        using var activity = commandActivityFactory.Create();
                        activity.Type = CommandType.Slash;

                        try
                        {
                            await slashCommandHandler.HandleAsync(interaction, activity);
                        }
                        catch (Exception e)
                        {
                            activity.SetError(e);
                            throw;
                        }
                    },
                    nameof(SlashCommandHandler)
                ));
                break;

            case MESSAGE_COMPONENT:
                _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                    async () => await messageComponentHandler.HandleAsync(interaction),
                    nameof(MessageComponentHandler)
                ));
                break;

            case MODAL_SUBMIT:
                _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                    async () => await modalInteractionHandler.HandleAsync(interaction),
                    nameof(MessageComponentHandler)
                ));
                break;

            default:
                logger.LogWarning("Unknown interaction type: {Interaction}", interaction);
                break;
        }

        return Task.CompletedTask;
    }
}
