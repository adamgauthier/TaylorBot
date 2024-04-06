using Discord;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IDisabledGuildChannelCommandRepository
{
    ValueTask<bool> IsGuildChannelCommandDisabledAsync(CommandChannel channel, CommandGuild guild, CommandMetadata command);
    ValueTask EnableInAsync(CommandChannel channel, CommandGuild guild, string commandName);
    ValueTask DisableInAsync(CommandChannel channel, CommandGuild guild, string commandName);
}

public class NotGuildChannelDisabledPrecondition(IDisabledGuildChannelCommandRepository disabledGuildChannelCommandRepository) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
            return new PreconditionPassed();

        var isDisabled = await disabledGuildChannelCommandRepository.IsGuildChannelCommandDisabledAsync(context.Channel, context.Guild, command.Metadata);

        var canRun = await new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageChannels).CanRunAsync(command, context);

        return isDisabled ?
            new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is disabled in {context.Channel.Id} on {context.Guild.FormatLog()}",
                UserReason: new(
                    $"""
                    You can't use `{command.Metadata.Name}` because it is disabled in {context.Channel.Mention}.
                    {(canRun is PreconditionPassed
                        ? $"You can re-enable it by typing </command channel-enable:909694280703016991> {command.Metadata.Name}."
                        : "Ask a moderator to re-enable it.")}
                    """
                )
            ) :
            new PreconditionPassed();
    }
}
