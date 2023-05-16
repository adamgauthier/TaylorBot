using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.MessageLogging.Domain.TextChannel;

public record MessageLogChannel(SnowflakeId ChannelId, TimeSpan? CacheExpiry);

public interface IMessageLoggingChannelRepository
{
    ValueTask<MessageLogChannel?> GetDeletedLogsChannelForGuildAsync(IGuild guild);
    ValueTask<MessageLogChannel?> GetEditedLogsChannelForGuildAsync(IGuild guild);
}
