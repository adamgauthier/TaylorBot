using Dapper;
using Discord;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;

namespace TaylorBot.Net.MemberLogging.Infrastructure;

public class MemberLoggingChannelRepository(PostgresConnectionFactory postgresConnectionFactory) : IMemberLoggingChannelRepository
{
    private sealed record LogChannelDto(string member_log_channel_id);

    public async ValueTask<LogChannel?> GetLogChannelForGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
            """
            SELECT member_log_channel_id FROM plus.member_log_channels
            WHERE guild_id = @GuildId AND EXISTS (
                SELECT FROM plus.plus_guilds
                WHERE state = 'enabled' AND plus.member_log_channels.guild_id = plus.plus_guilds.guild_id
            );
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return logChannel != null ? new LogChannel(new SnowflakeId(logChannel.member_log_channel_id)) : null;
    }
}
