using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.MessageLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MessageLogging.Domain
{
    public class MessageDeletedLoggerService
    {
        private readonly MessageLogChannelFinder _messageLogChannelFinder;
        private readonly MessageDeletedEmbedFactory _messageDeletedEmbedFactory;

        public MessageDeletedLoggerService(
            MessageLogChannelFinder messageLogChannelFinder,
            MessageDeletedEmbedFactory messageDeletedEmbedFactory)
        {
            _messageLogChannelFinder = messageLogChannelFinder;
            _messageDeletedEmbedFactory = messageDeletedEmbedFactory;
        }

        public async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel)
        {
            if (channel is ITextChannel textChannel)
            {
                var logTextChannel = await _messageLogChannelFinder.FindLogChannelAsync(textChannel.Guild);

                if (logTextChannel != null)
                    await logTextChannel.SendMessageAsync(embed: _messageDeletedEmbedFactory.CreateMessageDeleted(cachedMessage, textChannel));
            }
        }

        public async Task OnMessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, ISocketMessageChannel channel)
        {
            if (channel is ITextChannel textChannel)
            {
                var logTextChannel = await _messageLogChannelFinder.FindLogChannelAsync(textChannel.Guild);

                if (logTextChannel != null)
                {
                    foreach (var embed in _messageDeletedEmbedFactory.CreateMessageBulkDeleted(cachedMessages, textChannel))
                    {
                        await logTextChannel.SendMessageAsync(embed: embed);
                    }
                }
            }
        }
    }
}
