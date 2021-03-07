using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.MemberLogging.Domain.TextChannel
{
    public record LogChannel(SnowflakeId ChannelId);

    public interface IMemberLoggingChannelRepository
    {
        ValueTask<LogChannel?> GetLogChannelForGuildAsync(IGuild guild);
    }
}
