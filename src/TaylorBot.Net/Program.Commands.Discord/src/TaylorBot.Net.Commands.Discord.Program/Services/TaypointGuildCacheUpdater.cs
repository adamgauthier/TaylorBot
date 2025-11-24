using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public partial class TaypointGuildCacheUpdater(ILogger<TaypointGuildCacheUpdater> logger, ITaypointBalanceRepository taypointBalanceRepository)
{
    public async Task UpdateLastKnownPointCountAsync(DiscordUser user, long updatedCount)
    {
        if (!user.IsBot && user.TryGetMember(out var member))
        {
            var rowsAffected = await taypointBalanceRepository.UpdateLastKnownPointCountAsync(member, updatedCount);
            if (rowsAffected > 0)
            {
                LogLastKnownCountUpdated(rowsAffected);
            }
            else
            {
                LogLastKnownCountAlreadyUpToDate();
            }
        }
    }

    public ValueTask UpdateLastKnownPointCountsAsync(CommandGuild guild, IReadOnlyList<TaypointCountUpdate> updates)
    {
        if (updates.Count > 0)
        {
            LogUpdatingLastKnownPointCounts(updates.Count);
            return new(taypointBalanceRepository.UpdateLastKnownPointCountsAsync(guild, updates));
        }
        else
        {
            LogNoLastKnownPointCountsToUpdate();
            return new();
        }
    }

    public async Task UpdateLastKnownPointCountsForRecentlyActiveMembersAsync(CommandGuild guild)
    {
        var rowsAffected = await taypointBalanceRepository.UpdateLastKnownPointCountsForRecentlyActiveMembersAsync(guild);
        LogLastKnownCountsUpdatedForActiveMembers(rowsAffected);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Last known count for member updated ({RowsAffected})")]
    private partial void LogLastKnownCountUpdated(int rowsAffected);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Last known count for member was already up to date")]
    private partial void LogLastKnownCountAlreadyUpToDate();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Updating last known point counts for {Count} members")]
    private partial void LogUpdatingLastKnownPointCounts(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "No last known point counts to update")]
    private partial void LogNoLastKnownPointCountsToUpdate();

    [LoggerMessage(Level = LogLevel.Debug, Message = "{RowsAffected} last known counts updated for active members")]
    private partial void LogLastKnownCountsUpdatedForActiveMembers(int rowsAffected);
}
