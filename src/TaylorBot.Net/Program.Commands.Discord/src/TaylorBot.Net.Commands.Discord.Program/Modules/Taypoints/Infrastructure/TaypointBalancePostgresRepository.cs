using Dapper;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;

public class TaypointBalancePostgresRepository : ITaypointBalanceRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public TaypointBalancePostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    private class TaypointBalanceDto
    {
        public long taypoint_count { get; set; }
    }

    public async ValueTask<TaypointBalance> GetBalanceAsync(IUser user)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var balance = await connection.QuerySingleAsync<TaypointBalanceDto>(
            "SELECT taypoint_count FROM users.users WHERE user_id = @UserId;",
            new
            {
                UserId = $"{user.Id}",
            }
        );

        return new(balance.taypoint_count, null);
    }

    private class TaypointBalanceWithRankDto
    {
        public long taypoint_count { get; set; }
        public int rank { get; set; }
    }

    public async ValueTask<TaypointBalance> GetBalanceWithRankAsync(IGuildUser user)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var balance = await connection.QuerySingleAsync<TaypointBalanceWithRankDto>(
            """
            SELECT taypoint_count, rank FROM (
                SELECT taypoint_count, gm.user_id, rank() OVER (ORDER BY taypoint_count DESC) AS rank
                FROM guilds.guild_members AS gm JOIN users.users AS u ON u.user_id = gm.user_id
                WHERE gm.guild_id = @GuildId AND gm.alive = TRUE AND u.is_bot = FALSE
            ) AS ranked
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
                GuildId = $"{user.GuildId}",
            }
        );

        return new(balance.taypoint_count, balance.rank);
    }

    private class LeaderboardEntryDto
    {
        public string user_id { get; set; } = null!;
        public string username { get; set; } = null!;
        public long taypoint_count { get; set; }
        public long rank { get; set; }
    }

    public async ValueTask<IList<TaypointLeaderboardEntry>> GetLeaderboardAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var entries = await connection.QueryAsync<LeaderboardEntryDto>(
            """
            SELECT gm.user_id, u.username, taypoint_count, rank() OVER (ORDER BY taypoint_count DESC) AS rank
            FROM guilds.guild_members AS gm JOIN users.users AS u ON u.user_id = gm.user_id
            WHERE gm.guild_id = @GuildId AND gm.alive = TRUE AND u.is_bot = FALSE
            LIMIT 100;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return entries.Select(e => new TaypointLeaderboardEntry(
            new(e.user_id),
            e.username,
            e.taypoint_count,
            e.rank
        )).ToList();
    }
}
