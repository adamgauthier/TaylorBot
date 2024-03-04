using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;

public class TaypointBalancePostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ITaypointBalanceRepository
{
    private record TaypointBalanceDto(long taypoint_count);

    public async ValueTask<TaypointBalance> GetBalanceAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var balance = await connection.QuerySingleAsync<TaypointBalanceDto>(
            "SELECT taypoint_count FROM users.users WHERE user_id = @UserId;",
            new
            {
                UserId = $"{user.Id}",
            }
        );

        return new(balance.taypoint_count, null);
    }

    public async ValueTask UpdateLastKnownPointCountAsync(IGuildUser guildUser, long updatedCount)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members
            SET last_known_taypoint_count = @TaypointCount
            WHERE guild_id = @GuildId AND user_id = @UserId;
            """,
            new
            {
                GuildId = $"{guildUser.GuildId}",
                UserId = $"{guildUser.Id}",
                TaypointCount = updatedCount,
            }
        );
    }

    public async ValueTask<IList<TaypointLeaderboardEntry>> GetLeaderboardAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return (await connection.QueryAsync<TaypointLeaderboardEntry>(
            // We use the cached last_known_taypoint_count for fast ordering (always NULL for bots)
            // We also retrieve the actual taypoint_count for update later
            """
            SELECT leaderboard.user_id, username, last_known_taypoint_count, rank, taypoint_count FROM
            (
                SELECT user_id, last_known_taypoint_count, rank() OVER (ORDER BY last_known_taypoint_count DESC) AS rank
                FROM guilds.guild_members
                WHERE guild_id = @GuildId AND alive = TRUE AND last_known_taypoint_count IS NOT NULL
                ORDER BY last_known_taypoint_count DESC
                LIMIT 100
            ) leaderboard
            JOIN users.users AS u ON leaderboard.user_id = u.user_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        )).ToList();
    }

    public async ValueTask UpdateLastKnownPointCountsAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members AS gm
            SET last_known_taypoint_count = u.taypoint_count
            FROM users.users AS u
            WHERE gm.guild_id = @GuildId AND gm.user_id = u.user_id AND gm.alive = TRUE;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            },
            commandTimeout: (int)TimeSpan.FromMinutes(1).TotalSeconds
        );
    }
}
