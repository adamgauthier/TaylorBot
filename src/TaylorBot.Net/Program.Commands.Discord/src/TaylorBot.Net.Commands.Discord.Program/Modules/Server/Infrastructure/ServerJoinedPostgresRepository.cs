using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Infrastructure;

public class ServerJoinedPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IServerJoinedRepository
{
    public async Task<ServerJoined> GetRankedJoinedAsync(IGuildUser guildUser)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<ServerJoined>(
            """
            SELECT first_joined_at
            FROM guilds.guild_members
            WHERE guild_id = @GuildId
            AND user_id = @UserId;
            """,
            new
            {
                GuildId = $"{guildUser.GuildId}",
                UserId = $"{guildUser.Id}",
            }
        );
    }

    public async Task FixMissingJoinedDateAsync(IGuildUser guildUser)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members SET first_joined_at = @FirstJoinedAt
            WHERE guild_id = @GuildId AND user_id = @UserId AND first_joined_at IS NULL;
            """,
            new
            {
                GuildId = $"{guildUser.GuildId}",
                UserId = $"{guildUser.Id}",
                FirstJoinedAt = guildUser.JoinedAt,
            }
        );
    }

    public async Task<IList<JoinedTimelineEntry>> GetTimelineAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var entries = await connection.QueryAsync<JoinedTimelineEntry>(
            """
            SELECT u.user_id, u.username, first_joined_at, rank
            FROM (
                SELECT
                    first_joined_at,
                    user_id,
                    alive,
                    rank() OVER (ORDER BY first_joined_at ASC NULLS LAST) AS rank
                FROM guilds.guild_members
                WHERE guild_id = @GuildId
                LIMIT 150
            ) AS ranked
            INNER JOIN users.users u ON ranked.user_id = u.user_id
            WHERE alive = TRUE;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return entries.ToList();
    }
}
