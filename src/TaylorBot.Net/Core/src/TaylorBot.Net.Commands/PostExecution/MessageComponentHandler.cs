using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.PostExecution;

public record DiscordButtonComponent(
    string Id,
    string Token,
    InteractionCustomId CustomId,
    SnowflakeId MessageId,
    SnowflakeId UserId,
    SnowflakeId? GuildId,
    Interaction RawInteraction
) : IInteraction;

public interface IComponentHandlerInfo
{
    string CustomIdName { get; }
}
public record MessageHandlerInfo(string CustomIdName) : IComponentHandlerInfo;
public record ModalHandlerInfo(string CustomIdName) : IComponentHandlerInfo;

public interface IButtonComponentHandler
{
    IComponentHandlerInfo Info { get; }

    Task HandleAsync(DiscordButtonComponent button);
}

public interface IButtonHandler : IButtonComponentHandler
{
    abstract static CustomIdNames CustomIdName { get; }
}

public partial class MessageComponentHandler(IServiceProvider services, ILogger<MessageComponentHandler> logger)
{
    private readonly Dictionary<string, Func<DiscordButtonComponent, ValueTask>> _callbacks = [];

    private InteractionResponseClient CreateInteractionClient() => services.GetRequiredService<InteractionResponseClient>();

    public async ValueTask HandleAsync(Interaction interaction)
    {
        switch (interaction.data!.component_type!)
        {
            case 2:
                ArgumentNullException.ThrowIfNull(interaction.data.custom_id);
                ArgumentNullException.ThrowIfNull(interaction.message);

                DiscordButtonComponent button = new(
                    interaction.id,
                    interaction.token,
                    new(interaction.data.custom_id),
                    interaction.message.id,
                    interaction.user != null ? interaction.user.id : interaction.member!.user.id,
                    interaction.guild_id != null ? new(interaction.guild_id) : null,
                    interaction
                );

                if (button.CustomId.IsValid)
                {
                    var handler = services.GetKeyedService<IButtonComponentHandler>(button.CustomId.Name);
                    if (handler != null)
                    {
                        logger.LogInformation("Handling button component {ParsedName} with id {RawId}", button.CustomId.ParsedName, button.CustomId.RawId);

                        switch (handler.Info)
                        {
                            case MessageHandlerInfo _:
                                await CreateInteractionClient().SendComponentAckResponseWithoutLoadingMessageAsync(button);
                                await handler.HandleAsync(button);
                                break;

                            case ModalHandlerInfo _:
                                await handler.HandleAsync(button);
                                break;

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

    public void AddCallback(string customId, Func<DiscordButtonComponent, ValueTask> callback)
    {
        _callbacks.Add(customId, callback);
    }

    public void RemoveCallback(string customId)
    {
        _callbacks.Remove(customId);
    }
}
