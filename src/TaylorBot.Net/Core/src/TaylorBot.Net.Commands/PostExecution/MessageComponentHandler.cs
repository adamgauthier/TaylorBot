using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.PostExecution;

public record DiscordButtonComponent(
    ParsedInteraction Interaction,
    InteractionCustomId CustomId,
    Interaction.Message Message
);

public interface IComponentHandlerInfo
{
    string CustomIdName { get; }
    bool RequireOriginalUser { get; }
}
public record MessageHandlerInfo(string CustomIdName, IList<ICommandPrecondition>? Preconditions = null, bool RequireOriginalUser = false) : IComponentHandlerInfo;
public record ModalHandlerInfo(string CustomIdName, bool RequireOriginalUser = false) : IComponentHandlerInfo;

public interface IButtonComponentHandler
{
    IComponentHandlerInfo Info { get; }

    Task HandleAsync(DiscordButtonComponent button, RunContext context);
}

public interface IButtonHandler : IButtonComponentHandler
{
    abstract static CustomIdNames CustomIdName { get; }
}

public partial class MessageComponentHandler(
    IServiceProvider services,
    ILogger<MessageComponentHandler> logger,
    ICommandRunner commandRunner,
    RunContextFactory contextFactory)
{
    private readonly Dictionary<string, Func<DiscordButtonComponent, ValueTask>> _callbacks = [];

    private InteractionResponseClient CreateInteractionClient() => services.GetRequiredService<InteractionResponseClient>();

    private const int BUTTON_COMPONENT_TYPE = 2;

    public async ValueTask HandleAsync(Interaction interaction, CommandActivity activity)
    {
        switch (interaction.data?.component_type)
        {
            case BUTTON_COMPONENT_TYPE:
                var button = CreateButtonComponent(interaction, activity);
                if (button.CustomId.IsValid)
                {
                    var handler = services.GetKeyedService<IButtonComponentHandler>(button.CustomId.Name);
                    if (handler != null)
                    {
                        if (ShouldIgnoreForOriginalUser(button, activity, handler.Info))
                        {
                            return;
                        }

                        switch (handler.Info)
                        {
                            case MessageHandlerInfo messageHandler:
                                {
                                    await CreateInteractionClient().SendComponentAckResponseWithoutLoadingMessageAsync(button);

                                    var context = BuildContext(button, activity, wasAcknowledged: true);

                                    Command command = new(
                                        new($"{button.CustomId.ParsedName}", IsSlashCommand: false),
                                        RunAsync: async () =>
                                        {
                                            await handler.HandleAsync(button, context);
                                            return new EmptyResult();
                                        },
                                        Preconditions: messageHandler.Preconditions);

                                    var result = await commandRunner.RunInteractionAsync(command, context);
                                    switch (result)
                                    {
                                        case EmptyResult _:
                                            break;

                                        case PreconditionFailed failed:
                                            logger.LogWarning("Precondition failed running button message command {CommandName}: {Error}", command.Metadata.Name, failed.PrivateReason);
                                            await CreateInteractionClient().EditOriginalResponseAsync(button.Interaction, message: new(EmbedFactory.CreateError(failed.UserReason.Reason)));
                                            break;

                                        default:
                                            logger.LogWarning("Unhandled result running button message command {CommandName}: {Result}", command.Metadata.Name, result.GetType().FullName);
                                            break;
                                    }
                                    break;
                                }

                            case ModalHandlerInfo _:
                                {
                                    // We're creating a modal, we have to answer immediately (no ack), skipping preconditions for fast response
                                    // Submitting the modal will check for preconditions
                                    var context = BuildContext(button, activity, wasAcknowledged: false);
                                    await handler.HandleAsync(button, context);
                                    break;
                                }

                            default: throw new NotImplementedException(handler.GetType().FullName);
                        }
                    }
                    else
                    {
                        logger.LogWarning("Button component without handler: {Interaction}", interaction);
                    }
                }
                else
                {
                    if (_callbacks.TryGetValue(button.CustomId.RawId, out var callback))
                    {
                        await CreateInteractionClient().SendComponentAckResponseWithoutLoadingMessageAsync(button);

                        await callback(button);
                    }
                    else
                    {
                        logger.LogWarning("Button component without callback: {Interaction}", interaction);
                    }
                }
                break;

            default:
                logger.LogWarning("Unknown component type: {Interaction}", interaction);
                break;
        }
    }

    private static DiscordButtonComponent CreateButtonComponent(Interaction interaction, CommandActivity activity)
    {
        var parsed = ParsedInteraction.Parse(interaction, activity);
        ArgumentNullException.ThrowIfNull(parsed.Data.custom_id);
        ArgumentNullException.ThrowIfNull(interaction.message);

        return new(
            parsed,
            new(parsed.Data.custom_id),
            interaction.message
        );
    }

    private bool ShouldIgnoreForOriginalUser(DiscordButtonComponent button, CommandActivity activity, IComponentHandlerInfo info)
    {
        if (!info.RequireOriginalUser)
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(button.Message.interaction_metadata);
        if (button.Interaction.UserId != button.Message.interaction_metadata.user.id)
        {
            var context = contextFactory.BuildContext(button.Interaction, activity, wasAcknowledged: false);
            logger.LogWarning(
                "Ignoring {User} using button {ParsedName} ({CustomId}, {InteractionId}) on message {MessageInfo} in channel {ChannelId}{GuildInfo}, original user is {OriginalUser}",
                context.User.FormatLog(),
                button.CustomId.ParsedName,
                button.CustomId.RawId,
                button.Interaction.Id,
                button.Message.interaction_metadata.name != null ? $"{button.Message.id} from interaction '{button.Message.interaction_metadata.name}'" : $"{button.Message.id}",
                context.Channel.Id,
                context.Guild != null ? $" on {context.Guild.FormatLog()}" : "",
                button.Message.interaction_metadata.user.id
            );
            return true;
        }

        return false;
    }

    private RunContext BuildContext(DiscordButtonComponent button, CommandActivity activity, bool wasAcknowledged)
    {
        var context = contextFactory.BuildContext(button.Interaction, activity, wasAcknowledged);
        logger.LogInformation(
            "{User} using button {ParsedName} ({CustomId}, {InteractionId}) on message {MessageInfo} in channel {ChannelId}{GuildInfo}",
            context.User.FormatLog(),
            button.CustomId.ParsedName,
            button.CustomId.RawId,
            button.Interaction.Id,
            button.Message.interaction_metadata?.name != null ? $"{button.Message.id} from interaction '{button.Message.interaction_metadata.name}'" : $"{button.Message.id}",
            context.Channel.Id,
            context.Guild != null ? $" on {context.Guild.FormatLog()}" : ""
        );
        return context;
    }

    public void AddCallback(string customId, Func<DiscordButtonComponent, ValueTask> callback)
    {
        _callbacks.Add(customId, callback);
    }

    public void RemoveCallback(string customId)
    {
        _callbacks.Remove(customId);
    }
}
