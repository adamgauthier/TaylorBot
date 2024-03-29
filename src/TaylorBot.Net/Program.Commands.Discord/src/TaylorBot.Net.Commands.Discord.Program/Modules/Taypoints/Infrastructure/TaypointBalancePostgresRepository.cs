﻿using Dapper;
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
            // We also request actual taypoint_count so last_known_taypoint_count can be updated in the background
            """
            SELECT leaderboard.user_id, username, last_known_taypoint_count, rank, taypoint_count FROM
            (
                SELECT user_id, last_known_taypoint_count, rank() OVER (ORDER BY last_known_taypoint_count DESC) AS rank
                FROM guilds.guild_members
                WHERE guild_id = @GuildId AND alive = TRUE AND last_known_taypoint_count IS NOT NULL
                ORDER BY last_known_taypoint_count DESC
                LIMIT 500
            ) leaderboard
            JOIN users.users AS u ON leaderboard.user_id = u.user_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        )).ToList();
    }

    public async ValueTask UpdateLastKnownPointCountsAsync(IGuild guild, IReadOnlyList<TaypointCountUpdate> updates)
    {
        List<string> userIds = [];
        List<long> counts = [];
        foreach (var entry in updates)
        {
            userIds.Add(entry.UserId);
            counts.Add(entry.TaypointCount);
        }

        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members gm
            SET last_known_taypoint_count = updated_counts.last_known_taypoint_count
            FROM (SELECT
                unnest(@UserIds::text[]) user_id,
                unnest(@Counts::bigint[]) last_known_taypoint_count
            ) updated_counts
            WHERE guild_id = @GuildId AND gm.user_id = updated_counts.user_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                UserIds = userIds,
                Counts = counts,
            },
            commandTimeout: (int)TimeSpan.FromMinutes(1).TotalSeconds
        );
    }
}
