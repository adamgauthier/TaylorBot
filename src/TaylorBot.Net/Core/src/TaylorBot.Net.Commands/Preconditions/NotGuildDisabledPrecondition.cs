using Discord;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.Commands.Preconditions;

public record GuildCommandDisabled(bool IsDisabled, bool WasCacheHit);

public interface IDisabledGuildCommandRepository
{
    ValueTask<GuildCommandDisabled> IsGuildCommandDisabledAsync(CommandGuild guild, CommandMetadata command);
    ValueTask EnableInAsync(IGuild guild, string commandName);
    ValueTask DisableInAsync(IGuild guild, string commandName);
}

public class DisabledGuildCommandDomainService(
    TaskExceptionLogger taskExceptionLogger,
    IDisabledGuildCommandRepository disabledGuildCommandRepository,
    GuildTrackerDomainService guildTrackerDomainService)
{
    public async Task<bool> IsGuildCommandDisabledAsync(CommandGuild guild, CommandMetadata command, RunContext context)
    {
        var result = await disabledGuildCommandRepository.IsGuildCommandDisabledAsync(guild, command);
        if (!result.WasCacheHit && guild.Fetched != null)
        {
            // Take advantage of the cache miss to track guild name changes in the background
            _ = taskExceptionLogger.LogOnError(
                async () => await guildTrackerDomainService.TrackGuildAndNameAsync(guild.Fetched),
                nameof(guildTrackerDomainService.TrackGuildAndNameAsync)
            );
        }
        return result.IsDisabled;
    }
}

public class NotGuildDisabledPrecondition(DisabledGuildCommandDomainService disabledGuildCommandDomainService, UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
            return new PreconditionPassed();

        var isDisabled = await disabledGuildCommandDomainService.IsGuildCommandDisabledAsync(context.Guild, command.Metadata, context);

        var canRun = await userHasPermission.Create(GuildPermission.ManageGuild).CanRunAsync(command, context);

        return isDisabled ?
            new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is disabled in {context.Guild.FormatLog()}",
                UserReason: new(
                    $"""
                    You can't use {context.MentionCommand(command)} because it is disabled in this server 🚫
                    {(canRun is PreconditionPassed
                        ? $"You can re-enable it by typing </command server-enable:909694280703016991> {command.Metadata.Name} ✅"
                        : "Ask a moderator to re-enable it 🙏")}
                    """,
                    HideInPrefixCommands: true)
            ) :
            new PreconditionPassed();
    }
}
