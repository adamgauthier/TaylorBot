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

public class CommandPrefixDomainService(TaskExceptionLogger taskExceptionLogger, ICommandPrefixRepository commandPrefixRepository, GuildTrackerDomainService guildTrackerDomainService)
{
    public async Task<string> GetPrefixAsync(IGuild? guild)
    {
        if (guild != null)
        {
            var result = await commandPrefixRepository.GetOrInsertGuildPrefixAsync(guild);
            if (result.GuildAdded.WasAdded || result.GuildAdded.WasGuildNameChanged)
            {
                _ = taskExceptionLogger.LogOnError(
                    async () => await guildTrackerDomainService.TrackGuildNameAsync(guild, result.GuildAdded),
                    nameof(guildTrackerDomainService.TrackGuildNameAsync)
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
