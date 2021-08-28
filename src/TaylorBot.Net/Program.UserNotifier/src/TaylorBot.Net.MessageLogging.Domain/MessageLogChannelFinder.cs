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

        public async Task<ITextChannel?> FindDeletedLogChannelAsync(IGuild guild)
        {
            var logChannel = await _messageLoggingChannelRepository.GetDeletedLogsChannelForGuildAsync(guild);

            return logChannel != null ?
                (await guild.GetTextChannelsAsync()).FirstOrDefault(c => logChannel.ChannelId.Id == c.Id) :
                null;
        }

        public Task<ITextChannel?> FindEditedLogChannelAsync(IGuild guild)
        {
            // Retrieve from repository
            return Task.FromResult((ITextChannel?)null);
        }
    }
}
