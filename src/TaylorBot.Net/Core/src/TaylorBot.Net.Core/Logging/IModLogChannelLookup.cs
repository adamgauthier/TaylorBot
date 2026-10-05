using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Logging;

public interface IModLogChannelLookup
{
    ValueTask<SnowflakeId?> GetChannelIdAsync(SnowflakeId guildId);
}
