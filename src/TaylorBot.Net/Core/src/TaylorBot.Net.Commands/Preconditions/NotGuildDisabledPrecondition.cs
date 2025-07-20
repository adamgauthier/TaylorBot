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

public class NotGuildDisabledPrecondition(
    DisabledGuildCommandDomainService disabledGuildCommandDomainService,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    IDisabledGuildCommandRepository disabledGuildCommandRepository,
    CommandMentioner mention) : ICommandPrecondition
{
    private readonly UserHasPermissionOrOwnerPrecondition userHasManageGuild = userHasPermission.Create(GuildPermission.ManageGuild);

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
        {
            return new PreconditionPassed();
        }

        var isDisabled = await disabledGuildCommandDomainService.IsGuildCommandDisabledAsync(context.Guild, command.Metadata, context);
        if (isDisabled)
        {
            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is disabled in {context.Guild.FormatLog()}",
                UserReason: new(
                    $"""
                    You can't use {mention.Command(command, context)} because it is disabled in this server 🚫
                    {(await userHasManageGuild.CanRunAsync(command, context) is PreconditionPassed
                        ? $"You can re-enable it by typing {mention.SlashCommand("command server-enable")} {command.Metadata.Name} ✅"
                        : "Ask a moderator to re-enable it 🙏")}
                    """,
                    HideInPrefixCommands: true)
            );
        }

        if (context.PrefixCommand != null)
        {
            var result = await disabledGuildCommandRepository.IsGuildCommandDisabledAsync(context.Guild, new("all-prefix"));
            var arePrefixCommandsDisabled = result.IsDisabled;
            if (arePrefixCommandsDisabled)
            {
                return new PreconditionFailed(
                    PrivateReason: $"Prefix commands disabled in {context.Guild.FormatLog()}",
                    UserReason: new(
                        $"""
                        You can't use {mention.Command(command, context)} because prefix commands are disabled in this server 🚫
                        {(context.PrefixCommand.ReplacementSlashCommands != null && context.PrefixCommand.ReplacementSlashCommands.Count > 1
                            ? $"Use these slash commands instead ⚡\n{string.Join('\n', context.PrefixCommand.ReplacementSlashCommands.Select(c => $"👉 {mention.SlashCommand(c, context)} 👈"))}"
                            : context.PrefixCommand.ReplacementSlashCommands != null && context.PrefixCommand.ReplacementSlashCommands.Count == 1
                                ? $"Use the slash command 👉 {mention.SlashCommand(context.PrefixCommand.ReplacementSlashCommands[0], context)} 👈 instead ⚡"
                                : $"Sorry, slash commands starting with **/** are the future of commands on Discord 😕")}
                        """)
                );
            }
        }

        return new PreconditionPassed();
    }
}
