using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.MemberLogging.Domain.TextChannel
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
