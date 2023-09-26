using Dapper;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Infrastructure;

public class ServerActivityPostgresRepository : IServerActivityRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public ServerActivityPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    public async Task<ServerMessages> GetMessagesAsync(IGuildUser guildUser)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<ServerMessages>(
            """
            SELECT message_count, word_count
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

    public async Task<IList<MessageLeaderboardEntry>> GetMessageLeaderboardAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var entries = await connection.QueryAsync<MessageLeaderboardEntry>(
            """
            SELECT gm.user_id, u.username, rank() OVER (ORDER BY message_count DESC) AS rank, message_count
            FROM guilds.guild_members AS gm JOIN users.users AS u ON u.user_id = gm.user_id
            WHERE gm.guild_id = @GuildId AND gm.alive = TRUE AND u.is_bot = FALSE
            LIMIT 100;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return entries.ToList();
    }

    public async Task<int> GetMinutesAsync(IGuildUser guildUser)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<int>(
            """
            SELECT minute_count, word_count
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

    public async Task<IList<MinuteLeaderboardEntry>> GetMinuteLeaderboardAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var entries = await connection.QueryAsync<MinuteLeaderboardEntry>(
            """
            SELECT gm.user_id, u.username, rank() OVER (ORDER BY minute_count DESC) AS rank, minute_count
            FROM guilds.guild_members AS gm JOIN users.users AS u ON u.user_id = gm.user_id
            WHERE gm.guild_id = @GuildId AND gm.alive = TRUE AND u.is_bot = FALSE
            LIMIT 100;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return entries.ToList();
    }
}
