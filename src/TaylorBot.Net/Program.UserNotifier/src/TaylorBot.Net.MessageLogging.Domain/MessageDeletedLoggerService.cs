using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MessageLogging.Domain
{
    public record CachedMessage(SnowflakeId Id, ICachedMessageData? Data);
    public interface ICachedMessageData { }
    public record DiscordNetCachedMessageData(IMessage Message) : ICachedMessageData;
    public record TaylorBotCachedMessageData(string AuthorTag, string AuthorId, MessageType? SystemMessageType, string? Content) : ICachedMessageData;

    public interface ICachedMessageRepository
    {
        ValueTask SaveMessageAsync(SnowflakeId messageId, TaylorBotCachedMessageData data);
        ValueTask<TaylorBotCachedMessageData?> GetMessageDataAsync(SnowflakeId messageId);
    }

    public class MessageDeletedLoggerService
    {
        private readonly MessageLogChannelFinder _messageLogChannelFinder;
        private readonly MessageDeletedEmbedFactory _messageDeletedEmbedFactory;
        private readonly ICachedMessageRepository _cachedMessageRepository;

        public MessageDeletedLoggerService(
            MessageLogChannelFinder messageLogChannelFinder,
            MessageDeletedEmbedFactory messageDeletedEmbedFactory,
            ICachedMessageRepository cachedMessageRepository
        )
        {
            _messageLogChannelFinder = messageLogChannelFinder;
            _messageDeletedEmbedFactory = messageDeletedEmbedFactory;
            _cachedMessageRepository = cachedMessageRepository;
        }

        private async ValueTask<ICachedMessageData?> GetCachedMessageDataAsync(Cacheable<IMessage, ulong> cachedMessage)
        {
            if (cachedMessage.Value != null)
            {
                return new DiscordNetCachedMessageData(cachedMessage.Value);
            }
            else
            {
                var messageData = await _cachedMessageRepository.GetMessageDataAsync(new(cachedMessage.Id));
                return messageData;
            }
        }

        public async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel)
        {
            if (channel is ITextChannel textChannel)
            {
                var logTextChannel = await _messageLogChannelFinder.FindLogChannelAsync(textChannel.Guild);

                if (logTextChannel != null)
                {
                    CachedMessage message = new(new(cachedMessage.Id), await GetCachedMessageDataAsync(cachedMessage));

                    await logTextChannel.SendMessageAsync(embed: _messageDeletedEmbedFactory.CreateMessageDeleted(message, textChannel));
                }
            }
        }

        public async Task OnMessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, ISocketMessageChannel channel)
        {
            if (channel is ITextChannel textChannel)
            {
                var logTextChannel = await _messageLogChannelFinder.FindLogChannelAsync(textChannel.Guild);

                if (logTextChannel != null)
                {
                    List<CachedMessage> messages = new();
                    foreach (var cachedMessage in cachedMessages)
                    {
                        messages.Add(new(new(cachedMessage.Id), await GetCachedMessageDataAsync(cachedMessage)));
                    }

                    foreach (var embed in _messageDeletedEmbedFactory.CreateMessageBulkDeleted(messages, textChannel))
                    {
                        await logTextChannel.SendMessageAsync(embed: embed);
                    }
                }
            }
        }

        public async Task OnGuildUserMessageReceivedAsync(SocketTextChannel textChannel, SocketMessage message)
        {
            var logTextChannel = await _messageLogChannelFinder.FindLogChannelAsync(textChannel.Guild);

            if (logTextChannel != null)
            {
                await _cachedMessageRepository.SaveMessageAsync(
                    new(message.Id),
                    new(
                        AuthorTag: $"{message.Author.Username}#{message.Author.Discriminator}",
                        AuthorId: message.Author.Id.ToString(),
                        SystemMessageType: message is ISystemMessage systemMessage ? systemMessage.Type : null,
                        Content: message is IUserMessage userMessage ? userMessage.Content : null
                    )
                );
            }
        }
    }
}
