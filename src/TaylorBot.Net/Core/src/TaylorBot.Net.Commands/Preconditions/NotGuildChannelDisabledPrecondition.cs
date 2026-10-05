using Discord;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IDisabledGuildChannelCommandRepository
{
    ValueTask<bool> IsGuildChannelCommandDisabledAsync(GuildTextChannel channel, CommandMetadata command);
    ValueTask EnableInAsync(GuildTextChannel channel, string commandName);
    ValueTask DisableInAsync(GuildTextChannel channel, string commandName);
}

public class NotGuildChannelDisabledPrecondition(IDisabledGuildChannelCommandRepository disabledGuildChannelCommandRepository, UserHasPermissionOrOwnerPrecondition.Factory userHasPermission, CommandMentioner mention) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null || context.GuildTextChannel == null)
            return new PreconditionPassed();

        var isDisabled = await disabledGuildChannelCommandRepository.IsGuildChannelCommandDisabledAsync(context.GuildTextChannel, command.Metadata);

        var canRun = await userHasPermission.Create(GuildPermission.ManageChannels).CanRunAsync(command, context);
        FormattableString hint = canRun switch
        {
            PreconditionPassed => $"You can re-enable it by typing {mention.Slash("command channel-enable")} {command.Metadata.Name} ✅",
            _ => $"Ask a moderator to re-enable it 🙏",
        };

        return isDisabled ?
            new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is disabled in {context.Channel.Id} on {context.Guild.FormatLog()}",
                UserReason: new(
                    await mention.FormatAsync(context, $"""
                    You can't use {mention.Command(command)} because it is disabled in {context.Channel.Mention} 🚫
                    {hint}
                    """)
                )
            ) :
            new PreconditionPassed();
    }
}
