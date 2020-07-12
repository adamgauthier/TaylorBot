using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.MessageLogging.Domain.TextChannel
{
    public class LogChannel
    {
        public SnowflakeId ChannelId { get; }

        public LogChannel(SnowflakeId id)
        {
            ChannelId = id;
        }
    }

    public interface IMessageLoggingChannelRepository
    {
        ValueTask<LogChannel?> GetMessageLogChannelForGuildAsync(IGuild guild);
    }
}
