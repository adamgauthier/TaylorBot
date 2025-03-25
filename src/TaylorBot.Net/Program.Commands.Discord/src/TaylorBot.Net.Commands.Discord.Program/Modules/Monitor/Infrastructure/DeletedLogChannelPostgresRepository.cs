using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Infrastructure;

public class DeletedLogChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IDeletedLogChannelRepository
{
    public async ValueTask AddOrUpdateDeletedLogAsync(GuildTextChannel textChannel)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO plus.deleted_log_channels (guild_id, deleted_log_channel_id)
            VALUES (@GuildId, @ChannelId)
            ON CONFLICT (guild_id) DO UPDATE SET
                deleted_log_channel_id = excluded.deleted_log_channel_id;
            """,
            new
            {
                GuildId = $"{textChannel.GuildId}",
                ChannelId = $"{textChannel.Id}",
            }
        );
    }

    private sealed class LogChannelDto
    {
        public string deleted_log_channel_id { get; set; } = null!;
    }

    public async ValueTask<DeletedLog?> GetDeletedLogForGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
            """
            SELECT deleted_log_channel_id FROM plus.deleted_log_channels
            WHERE guild_id = @GuildId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return logChannel != null ? new DeletedLog(new SnowflakeId(logChannel.deleted_log_channel_id)) : null;
    }

    public async ValueTask RemoveDeletedLogAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM plus.deleted_log_channels WHERE guild_id = @GuildId;",
            new
            {
                GuildId = $"{guild.Id}",
            }
        );
    }
}
