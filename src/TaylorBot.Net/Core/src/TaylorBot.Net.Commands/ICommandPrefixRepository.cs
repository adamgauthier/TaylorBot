using Discord;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.EntityTracker.Domain.Guild;

namespace TaylorBot.Net.Commands;

public record CommandPrefix(GuildAddedResult GuildAdded, string Prefix);

public interface ICommandPrefixRepository
{
    ValueTask<CommandPrefix> GetOrInsertGuildPrefixAsync(IGuild guild);
    ValueTask ChangeGuildPrefixAsync(IGuild guild, string prefix);
}

public class CommandPrefixDomainService
{
    private readonly TaskExceptionLogger _taskExceptionLogger;
    private readonly ICommandPrefixRepository _commandPrefixRepository;
    private readonly GuildTrackerDomainService _guildTrackerDomainService;

    public CommandPrefixDomainService(TaskExceptionLogger taskExceptionLogger, ICommandPrefixRepository commandPrefixRepository, GuildTrackerDomainService guildTrackerDomainService)
    {
        _taskExceptionLogger = taskExceptionLogger;
        _commandPrefixRepository = commandPrefixRepository;
        _guildTrackerDomainService = guildTrackerDomainService;
    }

    public async Task<string> GetPrefixAsync(IGuild? guild)
    {
        if (guild != null)
        {
            var result = await _commandPrefixRepository.GetOrInsertGuildPrefixAsync(guild);
            if (result.GuildAdded.WasAdded || result.GuildAdded.WasGuildNameChanged)
            {
                _ = _taskExceptionLogger.LogOnError(
                    async () => await _guildTrackerDomainService.TrackGuildNameAsync(guild, result.GuildAdded),
                    nameof(_guildTrackerDomainService.TrackGuildNameAsync)
                );
            }
            return result.Prefix;
        }
        else
        {
            return string.Empty;
        }
    }
}
