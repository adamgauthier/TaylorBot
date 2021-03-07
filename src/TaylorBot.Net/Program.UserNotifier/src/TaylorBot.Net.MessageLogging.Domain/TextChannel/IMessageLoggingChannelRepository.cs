using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.MessageLogging.Domain.TextChannel
{
    public record LogChannel(SnowflakeId ChannelId);

    public interface IMessageLoggingChannelRepository
    {
        ValueTask<LogChannel?> GetDeletedLogsChannelForGuildAsync(IGuild guild);
    }
}
