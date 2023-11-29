using Discord;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.Commands.Preconditions;

public record GuildCommandDisabled(bool IsDisabled, bool WasCacheHit);

public interface IDisabledGuildCommandRepository
{
    ValueTask<GuildCommandDisabled> IsGuildCommandDisabledAsync(IGuild guild, CommandMetadata command);
    ValueTask EnableInAsync(IGuild guild, string commandName);
    ValueTask DisableInAsync(IGuild guild, string commandName);
}

public class DisabledGuildCommandDomainService
{
    private readonly TaskExceptionLogger _taskExceptionLogger;
    private readonly IDisabledGuildCommandRepository _disabledGuildCommandRepository;
    private readonly GuildTrackerDomainService _guildTrackerDomainService;
    private readonly Lazy<ITaylorBotClient> _taylorBotClient;

    public DisabledGuildCommandDomainService(
        TaskExceptionLogger taskExceptionLogger,
        IDisabledGuildCommandRepository disabledGuildCommandRepository,
        GuildTrackerDomainService guildTrackerDomainService,
        Lazy<ITaylorBotClient> taylorBotClient)
    {
        _taskExceptionLogger = taskExceptionLogger;
        _disabledGuildCommandRepository = disabledGuildCommandRepository;
        _guildTrackerDomainService = guildTrackerDomainService;
        _taylorBotClient = taylorBotClient;
    }

    public async Task<bool> IsGuildCommandDisabledAsync(IGuild guild, CommandMetadata command, RunContext context)
    {
        var result = await _disabledGuildCommandRepository.IsGuildCommandDisabledAsync(guild, command);
        if (!result.WasCacheHit)
        {
            // Take advantage of the cache miss to track guild name changes in the background
            _ = _taskExceptionLogger.LogOnError(
                async () =>
                {
                    if (context.IsFakeGuild)
                    {
                        var realGuild = _taylorBotClient.Value.ResolveRequiredGuild(guild.Id);
                        await _guildTrackerDomainService.TrackGuildAndNameAsync(realGuild);
                    }
                    else
                    {
                        await _guildTrackerDomainService.TrackGuildAndNameAsync(guild);
                    }
                },
                nameof(_guildTrackerDomainService.TrackGuildAndNameAsync)
            );

        }
        return result.IsDisabled;
    }
}

public class NotGuildDisabledPrecondition : ICommandPrecondition
{
    private readonly DisabledGuildCommandDomainService _disabledGuildCommandDomainService;

    public NotGuildDisabledPrecondition(DisabledGuildCommandDomainService disabledGuildCommandDomainService)
    {
        _disabledGuildCommandDomainService = disabledGuildCommandDomainService;
    }

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
            return new PreconditionPassed();

        var isDisabled = await _disabledGuildCommandDomainService.IsGuildCommandDisabledAsync(context.Guild, command.Metadata, context);

        var canRun = await new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild).CanRunAsync(command, context);

        return isDisabled ?
            new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is disabled in {context.Guild.FormatLog()}",
                UserReason: new(
                    string.Join('\n',
                        new[] {
                            $"You can't use `{command.Metadata.Name}` because it is disabled in this server.",
                            canRun is PreconditionPassed
                                ? $"You can re-enable it by typing </command server-enable:909694280703016991> {command.Metadata.Name}."
                                : "Ask a moderator to re-enable it."
                        }),
                    HideInPrefixCommands: true
                )
            ) :
            new PreconditionPassed();
    }
}
