using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public class TaypointGuildCacheUpdater(ILogger<TaypointGuildCacheUpdater> logger, ITaypointBalanceRepository taypointBalanceRepository, TaskExceptionLogger taskExceptionLogger)
{
    public void UpdateLastKnownPointCountInBackground(IUser user, long updatedCount)
    {
        if (!user.IsBot && user is IGuildUser guildUser)
        {
            _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                async () => await taypointBalanceRepository.UpdateLastKnownPointCountAsync(guildUser, updatedCount),
                nameof(taypointBalanceRepository.UpdateLastKnownPointCountAsync)
            ));
        }
    }

    public void UpdateLastKnownPointCountsInBackground(IGuild guild, IReadOnlyList<TaypointCountUpdate> updates)
    {
        if (updates.Count > 0)
        {
            logger.LogDebug("Updating last known point counts for {Count} members", updates.Count);

            _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                async () => await taypointBalanceRepository.UpdateLastKnownPointCountsAsync(guild, updates),
                nameof(taypointBalanceRepository.UpdateLastKnownPointCountsAsync)
            ));
        }
        else
        {
            logger.LogDebug("No last known point counts to update");
        }
    }
}
