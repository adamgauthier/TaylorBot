using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.EntityTracker.Domain.TextChannel
{
    public class LogChannel
    {
        public SnowflakeId ChannelId { get; }

        public LogChannel(SnowflakeId id)
        {
            ChannelId = id;
        }
    }
}
