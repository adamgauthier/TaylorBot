using Discord;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Domain
{
    public class MessageLogChannelFinder
    {
        private readonly IMessageLoggingChannelRepository _messageLoggingChannelRepository;

        public MessageLogChannelFinder(IMessageLoggingChannelRepository messageLoggingChannelRepository)
        {
            _messageLoggingChannelRepository = messageLoggingChannelRepository;
        }

        public async Task<ITextChannel> FindLogChannelAsync(IGuild guild)
        {
            var logChannels = await _messageLoggingChannelRepository.GetMessageLogChannelsForGuildAsync(guild);
            var textChannels = await guild.GetTextChannelsAsync();

            return textChannels.FirstOrDefault(channel =>
                logChannels.Any(logChannel => logChannel.ChannelId.Id == channel.Id)
            );
        }
    }
}
