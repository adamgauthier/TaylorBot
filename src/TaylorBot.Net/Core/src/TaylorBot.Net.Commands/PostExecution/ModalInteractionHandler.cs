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

public record ModalCallback(Func<ModalSubmit, ValueTask> SubmitAsync, bool IsPrivateResponse);

public class ModalInteractionHandler(
    IServiceProvider services,
    ILogger<ModalInteractionHandler> logger,
    ICommandRunner commandRunner,
    RunContextFactory contextFactory)
{
    private readonly Dictionary<string, ModalCallback> _callbacks = [];

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

                Command command = new(
                    new($"{submit.CustomId.ParsedName}"),
                    RunAsync: async () =>
                    {
                        await handler.HandleAsync(submit);
                        return new EmptyResult();
                    },
                    Preconditions: handler.Info.Preconditions);
                var context = contextFactory.BuildContext(submit.Interaction, activity, wasAcknowledged: true);

                var result = await commandRunner.RunInteractionAsync(command, context);
                switch (result)
                {
                    case EmptyResult _:
                        break;

                    case PreconditionFailed failed:
                        logger.LogWarning("Precondition failed running modal command {CommandName}: {Error}", command.Metadata.Name, failed.PrivateReason);
                        await CreateInteractionClient().EditOriginalResponseAsync(submit.Interaction, message: new(EmbedFactory.CreateError(failed.UserReason.Reason)));
                        break;

                    default:
                        logger.LogWarning("Unhandled result running modal command {CommandName}: {Result}", command.Metadata.Name, result.GetType().FullName);
                        break;
                }
            }
            else
            {
                logger.LogWarning("Modal create without handler: {Interaction}", interaction);
            }
        }
        else if (_callbacks.TryGetValue(submit.CustomId.RawId, out var callback))
        {
            await CreateInteractionClient().SendAckResponseWithLoadingMessageAsync(submit, callback.IsPrivateResponse);

            await callback.SubmitAsync(submit);
        }
        else
        {
            logger.LogWarning("Modal create without callback: {Interaction}", interaction);
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
            parsed.Data.components
                .Select(c =>
                {
                    var component = c.components?[0];
                    ArgumentNullException.ThrowIfNull(component);
                    ArgumentNullException.ThrowIfNull(component.custom_id);
                    ArgumentNullException.ThrowIfNull(component.value);

                    return new TextInputSubmit(component.custom_id, component.value);
                })
                .ToList()
        );
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
