using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using static TaylorBot.Net.Commands.PostExecution.ModalSubmit;

namespace TaylorBot.Net.Commands.PostExecution;

public record ModalSubmit(
    ParsedInteraction Interaction,
    string Token,
    InteractionCustomId CustomId,
    IReadOnlyList<TextInputSubmit> TextInputs
)
{
    public record TextInputSubmit(string CustomId, string Value);
}

public record ModalComponentHandlerInfo(bool IsPrivateResponse, IList<ICommandPrecondition>? Preconditions = null);

public interface IModalComponentHandler
{
    ModalComponentHandlerInfo Info { get; }

    Task HandleAsync(ModalSubmit submit);
}

public interface IModalHandler : IModalComponentHandler
{
    abstract static CustomIdNames CustomIdName { get; }
}

public class ModalInteractionHandler(
    IServiceProvider services,
    ILogger<ModalInteractionHandler> logger,
    ICommandRunner commandRunner,
    RunContextFactory contextFactory)
{
    private InteractionResponseClient CreateInteractionClient() => services.GetRequiredService<InteractionResponseClient>();

    public async ValueTask HandleAsync(Interaction interaction, CommandActivity activity)
    {
        var submit = CreateModalSubmit(interaction, activity);
        if (submit.CustomId.IsValid)
        {
            var handler = services.GetKeyedService<IModalComponentHandler>(submit.CustomId.Name);
            if (handler != null)
            {
                await CreateInteractionClient().SendAckResponseWithLoadingMessageAsync(submit, handler.Info.IsPrivateResponse);
                logger.LogInformation("Handling modal component {ParsedName} with id {RawId}", submit.CustomId.ParsedName, submit.CustomId.RawId);

                var messageResponse = await RunInteractionAsync(activity, submit, handler);
                if (messageResponse != null)
                {
                    await CreateInteractionClient().EditOriginalResponseAsync(submit.Interaction, message: messageResponse);
                }
            }
            else
            {
                logger.LogWarning("Modal create without handler: {Interaction}", interaction);
            }
        }
        else
        {
            logger.LogWarning("Modal create without callback: {Interaction}", interaction);
        }
    }

    private async Task<MessageResponse?> RunInteractionAsync(CommandActivity activity, ModalSubmit submit, IModalComponentHandler handler)
    {
        Command command = new(
            new($"{submit.CustomId.ParsedName}", IsSlashCommand: false),
            RunAsync: async () =>
            {
                await handler.HandleAsync(submit);
                // We're letting the handler use InteractionResponseClient directly
                return new EmptyResult();
            },
            Preconditions: handler.Info.Preconditions);

        var context = contextFactory.BuildContext(submit.Interaction, activity, wasAcknowledged: true);

        try
        {
            var result = await commandRunner.RunInteractionAsync(command, context);

            switch (result)
            {
                case EmptyResult:
                    return null;

                case PreconditionFailed failed:
                    return new(EmbedFactory.CreateError(failed.UserReason.Reason));

                default:
                    logger.LogWarning("Unhandled result running modal command {CommandName}: {Result}", command.Metadata.Name, result.GetType().FullName);
                    return null;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception in modal submit {Id} action:", submit.Interaction.Id);
            return new(EmbedFactory.CreateError("Oops, an unknown error occurred. Sorry about that 😕"));
        }
    }

    private static ModalSubmit CreateModalSubmit(Interaction interaction, CommandActivity activity)
    {
        var parsed = ParsedInteraction.Parse(interaction, activity);
        ArgumentNullException.ThrowIfNull(parsed.Data.custom_id);
        ArgumentNullException.ThrowIfNull(parsed.Data.components);

        return new(
            parsed,
            interaction.token,
            new(parsed.Data.custom_id),
            [.. parsed.Data.components
                .Select(c =>
                {
                    var component = c.components?[0];
                    ArgumentNullException.ThrowIfNull(component);
                    ArgumentNullException.ThrowIfNull(component.custom_id);
                    ArgumentNullException.ThrowIfNull(component.value);

                    return new TextInputSubmit(component.custom_id, component.value);
                })]
        );
    }
}
