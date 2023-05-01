using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;

public record TaypointBalance(long TaypointCount, int? ServerRank);

public record TaypointLeaderboardEntry(SnowflakeId UserId, string Username, long TaypointCount, long Rank);

public interface ITaypointBalanceRepository
{
    ValueTask<TaypointBalance> GetBalanceAsync(IUser user);
    ValueTask<TaypointBalance> GetBalanceWithRankAsync(IGuildUser user);
    ValueTask<IList<TaypointLeaderboardEntry>> GetLeaderboardAsync(IGuild guild);
}
