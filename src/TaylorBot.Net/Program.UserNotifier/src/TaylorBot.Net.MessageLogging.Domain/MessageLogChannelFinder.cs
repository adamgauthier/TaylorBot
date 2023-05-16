using Discord;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Domain;

public record FoundChannel(MessageLogChannel Channel, ITextChannel Resolved);

public class MessageLogChannelFinder
{
    private readonly IMessageLoggingChannelRepository _messageLoggingChannelRepository;

    public MessageLogChannelFinder(IMessageLoggingChannelRepository messageLoggingChannelRepository)
    {
        _messageLoggingChannelRepository = messageLoggingChannelRepository;
    }

    public async ValueTask<FoundChannel?> FindDeletedLogChannelAsync(IGuild guild)
    {
        var logChannel = await _messageLoggingChannelRepository.GetDeletedLogsChannelForGuildAsync(guild);
        if (logChannel == null)
        {
            return null;
        }

        var resolved = (await guild.GetTextChannelsAsync()).FirstOrDefault(c => logChannel.ChannelId.Id == c.Id);
        if (resolved == null)
        {
            return null;
        }

        return new(logChannel, resolved);
    }

    public async ValueTask<FoundChannel?> FindEditedLogChannelAsync(IGuild guild)
    {
        var logChannel = await _messageLoggingChannelRepository.GetEditedLogsChannelForGuildAsync(guild);
        if (logChannel == null)
        {
            return null;
        }

        var resolved = (await guild.GetTextChannelsAsync()).FirstOrDefault(c => logChannel.ChannelId.Id == c.Id);
        if (resolved == null)
        {
            return null;
        }

        return new(logChannel, resolved);
    }
}
