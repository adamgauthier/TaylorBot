using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.PostExecution;

public interface IDiscordMessageComponent
{
    ParsedInteraction Interaction { get; }
    InteractionCustomId CustomId { get; }
    Interaction.Message Message { get; }
}

public record DiscordButtonComponent(
    ParsedInteraction Interaction,
    InteractionCustomId CustomId,
    Interaction.Message Message
) : IDiscordMessageComponent;

public record DiscordUserSelectComponent(
    ParsedInteraction Interaction,
    InteractionCustomId CustomId,
    Interaction.Message Message,
    IReadOnlyList<DiscordUser> SelectedUsers
) : IDiscordMessageComponent;

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

public interface IUserSelectComponentHandler
{
    IComponentHandlerInfo Info { get; }
    Task HandleAsync(DiscordUserSelectComponent userSelect, RunContext context);
}

public interface IButtonHandler : IButtonComponentHandler
{
    abstract static CustomIdNames CustomIdName { get; }
}

public interface IUserSelectHandler : IUserSelectComponentHandler
{
    abstract static CustomIdNames CustomIdName { get; }
}

public partial class MessageComponentHandler(
    IServiceProvider services,
    ILogger<MessageComponentHandler> logger,
    ICommandRunner commandRunner,
    RunContextFactory contextFactory,
    InteractionMapper interactionMapper)
{
    private readonly Dictionary<string, Func<DiscordButtonComponent, ValueTask>> _callbacks = [];

    private IInteractionResponseClient CreateInteractionClient() => services.GetRequiredService<IInteractionResponseClient>();

    private const int BUTTON_COMPONENT_TYPE = 2;
    private const int USER_SELECT_COMPONENT_TYPE = 5;

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
                        bool acknowledged = false;
                        if (handler.Info is MessageHandlerInfo)
                        {
                            await CreateInteractionClient().SendComponentAckResponseWithoutLoadingMessageAsync(button);
                            acknowledged = true;
                        }

                        await ProcessParsedComponentAsync(
                            button,
                            handler.Info,
                            (context) => handler.HandleAsync(button, context),
                            activity,
                            acknowledged
                        );
                    }
                    else
                    {
                        logger.LogWarning("Button component without handler: {CustomId}, Interaction: {Interaction}", button.CustomId.RawId, interaction);
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
                        logger.LogWarning("Button component without callback: {CustomId}, Interaction: {Interaction}", button.CustomId.RawId, interaction);
                    }
                }
                break;

            case USER_SELECT_COMPONENT_TYPE:
                var userSelect = CreateUserSelectComponent(interaction, activity);
                if (userSelect.CustomId.IsValid)
                {
                    var handler = services.GetKeyedService<IUserSelectComponentHandler>(userSelect.CustomId.Name);
                    if (handler != null)
                    {
                        bool acknowledged = false;
                        if (handler.Info is MessageHandlerInfo)
                        {
                            await CreateInteractionClient().SendComponentAckResponseWithoutLoadingMessageAsync(userSelect);
                            acknowledged = true;
                        }

                        await ProcessParsedComponentAsync(
                            userSelect,
                            handler.Info,
                            (context) => handler.HandleAsync(userSelect, context),
                            activity,
                            acknowledged
                        );
                    }
                    else
                    {
                        logger.LogWarning("User select component without handler: {CustomId}, Interaction: {Interaction}", userSelect.CustomId.RawId, interaction);
                    }
                }
                else
                {
                    logger.LogWarning("User select component with invalid or unhandled custom ID: {CustomId}, Interaction: {Interaction}", userSelect.CustomId.RawId, interaction);
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

    private DiscordUserSelectComponent CreateUserSelectComponent(Interaction interaction, CommandActivity activity)
    {
        var parsed = ParsedInteraction.Parse(interaction, activity);
        ArgumentNullException.ThrowIfNull(parsed.Data.custom_id);
        ArgumentNullException.ThrowIfNull(parsed.Data.values);
        ArgumentNullException.ThrowIfNull(parsed.Data.resolved);
        ArgumentNullException.ThrowIfNull(parsed.Data.resolved.users);
        ArgumentNullException.ThrowIfNull(interaction.message);

        var selectedUsers = parsed.Data.values.Select(userId =>
        {
            if (!parsed.Data.resolved.users.TryGetValue(userId, out var resolvedUser))
            {
                throw new InvalidOperationException($"Can't find user {userId} in resolved data");
            }

            return interactionMapper.ToUser(
                resolvedUser,
                parsed.Data.resolved.members?.TryGetValue(userId, out var resolvedMember) == true ? resolvedMember : null
            );
        }).ToList();

        return new(
            parsed,
            new(parsed.Data.custom_id),
            interaction.message,
            selectedUsers
        );
    }

    private async ValueTask ProcessParsedComponentAsync<TComponent>(
        TComponent component,
        IComponentHandlerInfo handlerInfo,
        Func<RunContext, Task> handleAction,
        CommandActivity activity,
        bool wasAcknowledged) where TComponent : IDiscordMessageComponent
    {
        if (ShouldIgnoreForOriginalUser(component, activity, handlerInfo))
        {
            return;
        }

        var context = BuildContext(component, activity, wasAcknowledged);

        switch (handlerInfo)
        {
            case MessageHandlerInfo messageHandler:
                {
                    Command command = new(
                        new($"{component.CustomId.ParsedName}", IsSlashCommand: false),
                        RunAsync: async () =>
                        {
                            await handleAction(context);
                            return new EmptyResult();
                        },
                        Preconditions: messageHandler.Preconditions);

                    var result = await commandRunner.RunInteractionAsync(command, context);
                    switch (result)
                    {
                        case EmptyResult _:
                            break;

                        case PreconditionFailed failed:
                            logger.LogWarning("Precondition failed running component command {CommandName}: {Error}", command.Metadata.Name, failed.PrivateReason);
                            await CreateInteractionClient().EditOriginalResponseAsync(component.Interaction, message: new(EmbedFactory.CreateError(failed.UserReason.Reason)));
                            break;

                        default:
                            logger.LogWarning("Unhandled result running component command {CommandName}: {Result}", command.Metadata.Name, result.GetType().FullName);
                            break;
                    }
                    break;
                }

            case ModalHandlerInfo _:
                {
                    await handleAction(context);
                    break;
                }

            default:
                throw new NotImplementedException($"Handler info type not implemented: {handlerInfo.GetType().FullName} for component {component.CustomId.RawId}");
        }
    }

    private bool ShouldIgnoreForOriginalUser(IDiscordMessageComponent component, CommandActivity activity, IComponentHandlerInfo info)
    {
        if (!info.RequireOriginalUser)
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(component.Message.interaction_metadata);
        if (component.Interaction.UserId != component.Message.interaction_metadata.user.id)
        {
            var context = contextFactory.BuildContext(component.Interaction, activity, wasAcknowledged: false);
            logger.LogWarning(
                "Ignoring {User} using component {ParsedName} ({CustomId}, {InteractionId}) on message {MessageInfo} in channel {ChannelId}{GuildInfo}, original user is {OriginalUser}",
                context.User.FormatLog(),
                component.CustomId.ParsedName,
                component.CustomId.RawId,
                component.Interaction.Id,
                component.Message.interaction_metadata.name != null ? $"{component.Message.id} from interaction '{component.Message.interaction_metadata.name}'" : $"{component.Message.id}",
                context.Channel.Id,
                context.Guild != null ? $" on {context.Guild.FormatLog()}" : "",
                component.Message.interaction_metadata.user.id
            );
            return true;
        }

        return false;
    }

    private RunContext BuildContext(IDiscordMessageComponent component, CommandActivity activity, bool wasAcknowledged)
    {
        var context = contextFactory.BuildContext(component.Interaction, activity, wasAcknowledged);
        logger.LogInformation(
            "{User} using {Component} {ParsedName} ({CustomId}, {InteractionId}) on message {MessageInfo} in channel {ChannelId}{GuildInfo}",
            context.User.FormatLog(),
            component.GetType().Name,
            component.CustomId.ParsedName,
            component.CustomId.RawId,
            component.Interaction.Id,
            component.Message.interaction_metadata?.name != null ? $"{component.Message.id} from interaction '{component.Message.interaction_metadata.name}'" : $"{component.Message.id}",
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
