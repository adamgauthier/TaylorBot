using Dapper;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Infrastructure;

public class ModLogChannelLookup(PostgresConnectionFactory connectionFactory) : IModLogChannelLookup
{
    public async ValueTask<SnowflakeId?> GetChannelIdAsync(SnowflakeId guildId)
    {
        await using var connection = connectionFactory.CreateConnection();
        var channelId = await connection.QuerySingleOrDefaultAsync<string>(
            "SELECT channel_id FROM moderation.mod_log_channels WHERE guild_id = @GuildId;",
            new { GuildId = $"{guildId}" });

        return channelId == null ? null : new SnowflakeId(channelId);
    }
}
