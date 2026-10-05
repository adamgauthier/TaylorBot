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
    BackgroundTasks backgroundTasks,
    IDisabledGuildCommandRepository disabledGuildCommandRepository,
    GuildTrackerDomainService guildTrackerDomainService)
{
    public async Task<bool> IsGuildCommandDisabledAsync(CommandGuild guild, CommandMetadata command, RunContext context)
    {
        var result = await disabledGuildCommandRepository.IsGuildCommandDisabledAsync(guild, command);
        if (!result.WasCacheHit && guild.Fetched != null)
        {
            // Take advantage of the cache miss to track guild name changes in the background
            _ = backgroundTasks.Run(
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
            var canRun = await userHasManageGuild.CanRunAsync(command, context);
            FormattableString hint = canRun switch
            {
                PreconditionPassed => $"You can re-enable it by typing {mention.Slash("command server-enable")} {command.Metadata.Name} ✅",
                _ => $"Ask a moderator to re-enable it 🙏",
            };
            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is disabled in {context.Guild.FormatLog()}",
                UserReason: new(
                    await mention.FormatAsync(context, $"""
                    You can't use {mention.Command(command)} because it is disabled in this server 🚫
                    {hint}
                    """),
                    HideInPrefixCommands: true)
            );
        }

        if (context.PrefixCommand != null)
        {
            var result = await disabledGuildCommandRepository.IsGuildCommandDisabledAsync(context.Guild, new("all-prefix"));
            var arePrefixCommandsDisabled = result.IsDisabled;
            if (arePrefixCommandsDisabled)
            {
                FormattableString hint = context.PrefixCommand.ReplacementSlashCommands switch
                {
                    { Count: > 1 } replacements => $"Use these slash commands instead ⚡\n{mention.Join("\n", replacements.Select(c => (FormattableString)$"👉 {mention.Slash(c)} 👈"))}",
                    { Count: 1 } replacements => $"Use the slash command 👉 {mention.Slash(replacements[0])} 👈 instead ⚡",
                    _ => $"Sorry, slash commands starting with **/** are the future of commands on Discord 😕",
                };
                return new PreconditionFailed(
                    PrivateReason: $"Prefix commands disabled in {context.Guild.FormatLog()}",
                    UserReason: new(
                        await mention.FormatAsync(context, $"""
                        You can't use {mention.Command(command)} because prefix commands are disabled in this server 🚫
                        {hint}
                        """))
                );
            }
        }

        return new PreconditionPassed();
    }
}
