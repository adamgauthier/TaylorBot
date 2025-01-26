using Dapper;
using Npgsql;
using TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Taypoints;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Infrastructure;

public class RollStatsPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IRollStatsRepository
{
    public async Task<RollProfile?> GetProfileAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<RollProfile?>(
            """
            SELECT roll_count, perfect_roll_count
            FROM users.roll_stats
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async Task<IList<RollLeaderboardEntry>> GetLeaderboardAsync(CommandGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return (await connection.QueryAsync<RollLeaderboardEntry>(
            // Querying for users with wins first, expectation is the row count will be lower than the guild members count for large guilds
            // Then we join to filter out users that are not part of the guild and get the top 100
            // Finally we join on users to get their latest username
            """
            SELECT leaderboard.user_id, username, perfect_roll_count, rank FROM
            (
                SELECT roll_users.user_id, perfect_roll_count, rank() OVER (ORDER BY perfect_roll_count DESC) AS rank FROM
                (
                    SELECT user_id, perfect_roll_count
                    FROM users.roll_stats
                    WHERE perfect_roll_count > 0
                ) roll_users
                JOIN guilds.guild_members AS gm ON roll_users.user_id = gm.user_id AND gm.guild_id = @GuildId AND gm.alive = TRUE
                ORDER BY perfect_roll_count DESC
                LIMIT 150
            ) leaderboard
            JOIN users.users AS u ON leaderboard.user_id = u.user_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        )).ToList();
    }

    public async Task WinRollAsync(DiscordUser user, long taypointReward)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await AddRollCountAsync(connection, user);

        await TaypointPostgresUtil.AddTaypointsAsync(connection, user.Id, taypointReward);

        transaction.Commit();
    }

    public async Task WinPerfectRollAsync(DiscordUser user, long taypointReward)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            """
            INSERT INTO users.roll_stats (user_id, roll_count, perfect_roll_count)
            VALUES (@UserId, @RollCount, @RollCount)
            ON CONFLICT (user_id) DO UPDATE SET
                roll_count = roll_stats.roll_count + @RollCount,
                perfect_roll_count = roll_stats.perfect_roll_count + @RollCount;
            """,
            new
            {
                UserId = $"{user.Id}",
                RollCount = 1,
            }
        );

        await TaypointPostgresUtil.AddTaypointsAsync(connection, user.Id, taypointReward);

        transaction.Commit();
    }

    public async Task AddRollCountAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await AddRollCountAsync(connection, user);
    }

    private static async Task AddRollCountAsync(NpgsqlConnection connection, DiscordUser user)
    {
        await connection.ExecuteAsync(
            """
            INSERT INTO users.roll_stats (user_id, roll_count)
            VALUES (@UserId, @RollCount)
            ON CONFLICT (user_id) DO UPDATE SET
                roll_count = roll_stats.roll_count + @RollCount;
            """,
            new
            {
                UserId = $"{user.Id}",
                RollCount = 1,
            }
        );
    }
}
