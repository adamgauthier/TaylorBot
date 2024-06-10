using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public class TaypointGuildCacheUpdater(ILogger<TaypointGuildCacheUpdater> logger, ITaypointBalanceRepository taypointBalanceRepository)
{
    public async Task UpdateLastKnownPointCountAsync(DiscordUser user, long updatedCount)
    {
        if (!user.IsBot && user.TryGetMember(out var member))
        {
            var rowsAffected = await taypointBalanceRepository.UpdateLastKnownPointCountAsync(member, updatedCount);
            if (rowsAffected > 0)
            {
                logger.LogDebug("Last known count for member updated ({RowsAffected})", rowsAffected);
            }
            else
            {
                logger.LogDebug("Last known count for member was already up to date");
            }
        }
    }

    public ValueTask UpdateLastKnownPointCountsAsync(CommandGuild guild, IReadOnlyList<TaypointCountUpdate> updates)
    {
        if (updates.Count > 0)
        {
            logger.LogDebug("Updating last known point counts for {Count} members", updates.Count);
            return new(taypointBalanceRepository.UpdateLastKnownPointCountsAsync(guild, updates));
        }
        else
        {
            logger.LogDebug("No last known point counts to update");
            return new();
        }
    }

    public async Task UpdateLastKnownPointCountsForRecentlyActiveMembersAsync(CommandGuild guild)
    {
        var rowsAffected = await taypointBalanceRepository.UpdateLastKnownPointCountsForRecentlyActiveMembersAsync(guild);
        logger.LogDebug("{RowsAffected} last known counts updated for active members", rowsAffected);
    }
}
