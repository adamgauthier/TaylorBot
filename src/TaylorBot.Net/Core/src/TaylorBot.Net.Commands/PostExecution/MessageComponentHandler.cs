using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Core.Channel;
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

public record DiscordChannelSelectComponent(
    ParsedInteraction Interaction,
    InteractionCustomId CustomId,
    Interaction.Message Message,
    IReadOnlyList<DiscordChannel> SelectedChannels
) : IDiscordMessageComponent;

public record DiscordStringSelectComponent(
    ParsedInteraction Interaction,
    InteractionCustomId CustomId,
    Interaction.Message Message,
    IReadOnlyList<string> Values
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

public interface IChannelSelectComponentHandler
{
    IComponentHandlerInfo Info { get; }
    Task HandleAsync(DiscordChannelSelectComponent channelSelect, RunContext context);
}

public interface IStringSelectComponentHandler
{
    IComponentHandlerInfo Info { get; }
    Task HandleAsync(DiscordStringSelectComponent select, RunContext context);
}

public interface IButtonHandler : IButtonComponentHandler
{
    abstract static CustomIdNames CustomIdName { get; }
}

public interface IUserSelectHandler : IUserSelectComponentHandler
{
    abstract static CustomIdNames CustomIdName { get; }
}

public interface IChannelSelectHandler : IChannelSelectComponentHandler
{
    abstract static CustomIdNames CustomIdName { get; }
}

public interface IStringSelectHandler : IStringSelectComponentHandler
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
    private IInteractionResponseClient CreateInteractionClient() => services.GetRequiredService<IInteractionResponseClient>();

    public async ValueTask HandleAsync(Interaction interaction, CommandActivity activity)
    {
        switch (interaction.data?.component_type)
        {
            case (byte)InteractionComponentType.Button:
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
                        LogButtonWithoutHandler(button.CustomId.RawId, interaction);
                    }
                }
                else
                {
                    LogButtonWithInvalidCustomId(button.CustomId.RawId, interaction);
                }
                break;

            case (byte)InteractionComponentType.StringSelect:
                var stringSelect = CreateStringSelectComponent(interaction, activity);
                if (stringSelect.CustomId.IsValid)
                {
                    var handler = services.GetKeyedService<IStringSelectComponentHandler>(stringSelect.CustomId.Name);
                    if (handler != null)
                    {
                        bool acknowledged = false;
                        if (handler.Info is MessageHandlerInfo)
                        {
                            await CreateInteractionClient().SendComponentAckResponseWithoutLoadingMessageAsync(stringSelect);
                            acknowledged = true;
                        }

                        await ProcessParsedComponentAsync(
                            stringSelect,
                            handler.Info,
                            (context) => handler.HandleAsync(stringSelect, context),
                            activity,
                            acknowledged
                        );
                    }
                    else
                    {
                        LogStringSelectWithoutHandler(stringSelect.CustomId.RawId, interaction);
                    }
                }
                else
                {
                    LogStringSelectWithInvalidCustomId(stringSelect.CustomId.RawId, interaction);
                }
                break;

            case (byte)InteractionComponentType.UserSelect:
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
                        LogUserSelectWithoutHandler(userSelect.CustomId.RawId, interaction);
                    }
                }
                else
                {
                    LogUserSelectWithInvalidCustomId(userSelect.CustomId.RawId, interaction);
                }
                break;

            case (byte)InteractionComponentType.ChannelSelect:
                var channelSelect = CreateChannelSelectComponent(interaction, activity);
                if (channelSelect.CustomId.IsValid)
                {
                    var handler = services.GetKeyedService<IChannelSelectComponentHandler>(channelSelect.CustomId.Name);
                    if (handler != null)
                    {
                        bool acknowledged = false;
                        if (handler.Info is MessageHandlerInfo)
                        {
                            await CreateInteractionClient().SendComponentAckResponseWithoutLoadingMessageAsync(channelSelect);
                            acknowledged = true;
                        }

                        await ProcessParsedComponentAsync(
                            channelSelect,
                            handler.Info,
                            (context) => handler.HandleAsync(channelSelect, context),
                            activity,
                            acknowledged
                        );
                    }
                    else
                    {
                        LogChannelSelectWithoutHandler(channelSelect.CustomId.RawId, interaction);
                    }
                }
                else
                {
                    LogChannelSelectWithInvalidCustomId(channelSelect.CustomId.RawId, interaction);
                }
                break;

            default:
                LogUnknownComponentType(interaction);
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

    private DiscordStringSelectComponent CreateStringSelectComponent(Interaction interaction, CommandActivity activity)
    {
        var parsed = ParsedInteraction.Parse(interaction, activity);
        ArgumentNullException.ThrowIfNull(parsed.Data.custom_id);
        ArgumentNullException.ThrowIfNull(parsed.Data.values);
        ArgumentNullException.ThrowIfNull(interaction.message);

        return new(
            parsed,
            new(parsed.Data.custom_id),
            interaction.message,
            parsed.Data.values
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

            MemberMappingInfo? memberInfo = parsed.Guild != null && parsed.Data.resolved.members?.TryGetValue(userId, out var resolvedMember) == true
                ? new(resolvedMember, parsed.Guild.Id)
                : null;

            return interactionMapper.ToUser(resolvedUser, memberInfo);
        }).ToList();

        return new(
            parsed,
            new(parsed.Data.custom_id),
            interaction.message,
            selectedUsers
        );
    }

    private DiscordChannelSelectComponent CreateChannelSelectComponent(Interaction interaction, CommandActivity activity)
    {
        var parsed = ParsedInteraction.Parse(interaction, activity);
        ArgumentNullException.ThrowIfNull(parsed.Data.custom_id);
        ArgumentNullException.ThrowIfNull(parsed.Data.values);
        ArgumentNullException.ThrowIfNull(parsed.Data.resolved);
        ArgumentNullException.ThrowIfNull(parsed.Data.resolved.channels);
        ArgumentNullException.ThrowIfNull(interaction.message);

        var selectedChannels = parsed.Data.values.Select(channelId =>
        {
            if (!parsed.Data.resolved.channels.TryGetValue(channelId, out var resolvedChannel))
            {
                throw new InvalidOperationException($"Can't find channel {channelId} in resolved data");
            }

            return interactionMapper.ToChannel(resolvedChannel);
        }).ToList();

        return new(
            parsed,
            new(parsed.Data.custom_id),
            interaction.message,
            selectedChannels
        );
    }

    private async ValueTask ProcessParsedComponentAsync<TComponent>(
        TComponent component,
        IComponentHandlerInfo handlerInfo,
        Func<RunContext, Task> handleAction,
        CommandActivity activity,
        bool wasAcknowledged) where TComponent : IDiscordMessageComponent
    {
        var context = BuildContext(component, activity, wasAcknowledged);

        var preconditions = handlerInfo is MessageHandlerInfo messageHandler ? messageHandler.Preconditions : null;

        Command command = new(
            new($"{component.CustomId.ParsedName}", IsSlashCommand: false),
            RunAsync: async () =>
            {
                if (ShouldIgnoreForOriginalUser(component, activity, handlerInfo, context))
                {
                    return new EmptyResult();
                }

                await handleAction(context);
                // We're letting the handler use IInteractionResponseClient directly
                return new EmptyResult();
            },
            Preconditions: preconditions);

        var result = await commandRunner.RunInteractionAsync(command, context);
        switch (result)
        {
            case EmptyResult _:
                break;

            case PreconditionFailed failed:
                LogPreconditionFailedComponentCommand(command.Metadata.Name, failed.PrivateReason);
                await CreateInteractionClient().EditOriginalResponseAsync(component.Interaction, EmbedFactory.CreateErrorEmbed(failed.UserReason.Reason));
                break;

            default:
                LogUnhandledComponentCommandResult(command.Metadata.Name, result.GetType().FullName);
                break;
        }
    }

    private RunContext BuildContext(IDiscordMessageComponent component, CommandActivity activity, bool wasAcknowledged)
    {
        var context = contextFactory.BuildContext(component.Interaction, activity, wasAcknowledged);
        LogUserUsingComponent(
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

    private bool ShouldIgnoreForOriginalUser(IDiscordMessageComponent component, CommandActivity activity, IComponentHandlerInfo info, RunContext context)
    {
        if (!info.RequireOriginalUser)
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(component.Message.interaction_metadata);
        if (component.Interaction.UserId != component.Message.interaction_metadata.user.id)
        {
            LogIgnoringUserUsingComponent(
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

    [LoggerMessage(LogLevel.Information, "Handling component {ParsedName} ({CustomId}, {InteractionId}) from {User} in channel {ChannelId}{GuildInfo}")]
    private partial void LogHandlingComponent(string parsedName, string customId, string interactionId, string user, string channelId, string guildInfo);

    [LoggerMessage(LogLevel.Information, "{User} using {Component} {ParsedName} ({CustomId}, {InteractionId}) on message {MessageInfo} in channel {ChannelId}{GuildInfo}")]
    private partial void LogUserUsingComponent(string user, string component, CustomIdNames parsedName, string customId, string interactionId, string messageInfo, string channelId, string guildInfo);

    [LoggerMessage(LogLevel.Warning, "Button component without handler: {CustomId}, Interaction: {Interaction}")]
    private partial void LogButtonWithoutHandler(string customId, Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "Button component with invalid custom ID: {CustomId}, Interaction: {Interaction}")]
    private partial void LogButtonWithInvalidCustomId(string customId, Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "String select without handler: {CustomId}, Interaction: {Interaction}")]
    private partial void LogStringSelectWithoutHandler(string customId, Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "String select with invalid custom ID: {CustomId}, Interaction: {Interaction}")]
    private partial void LogStringSelectWithInvalidCustomId(string customId, Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "User select without handler: {CustomId}, Interaction: {Interaction}")]
    private partial void LogUserSelectWithoutHandler(string customId, Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "User select with invalid custom ID: {CustomId}, Interaction: {Interaction}")]
    private partial void LogUserSelectWithInvalidCustomId(string customId, Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "Channel select without handler: {CustomId}, Interaction: {Interaction}")]
    private partial void LogChannelSelectWithoutHandler(string customId, Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "Channel select with invalid custom ID: {CustomId}, Interaction: {Interaction}")]
    private partial void LogChannelSelectWithInvalidCustomId(string customId, Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "Unknown component type: {Interaction}")]
    private partial void LogUnknownComponentType(Interaction interaction);

    [LoggerMessage(LogLevel.Warning, "Precondition failed running component command {CommandName}: {Error}")]
    private partial void LogPreconditionFailedComponentCommand(string commandName, string error);

    [LoggerMessage(LogLevel.Warning, "Unhandled result running component command {CommandName}: {Result}")]
    private partial void LogUnhandledComponentCommandResult(string commandName, string? result);

    [LoggerMessage(LogLevel.Warning, "Ignoring {User} using component {ParsedName} ({CustomId}, {InteractionId}) on message {MessageInfo} in channel {ChannelId}{GuildInfo}, original user is {OriginalUser}")]
    private partial void LogIgnoringUserUsingComponent(string user, CustomIdNames parsedName, string customId, string interactionId, string messageInfo, string channelId, string guildInfo, string originalUser);
}
