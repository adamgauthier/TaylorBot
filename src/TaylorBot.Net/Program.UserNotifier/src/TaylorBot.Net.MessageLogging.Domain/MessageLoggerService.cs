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

    public class MessageLoggerService
    {
        private readonly MessageLogChannelFinder _messageLogChannelFinder;
        private readonly MessageLogEmbedFactory _messageLogEmbedFactory;
        private readonly ICachedMessageRepository _cachedMessageRepository;

        public MessageLoggerService(MessageLogChannelFinder messageLogChannelFinder, MessageLogEmbedFactory messageLogEmbedFactory, ICachedMessageRepository cachedMessageRepository)
        {
            _messageLogChannelFinder = messageLogChannelFinder;
            _messageLogEmbedFactory = messageLogEmbedFactory;
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

        public async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, IMessageChannel channel)
        {
            if (channel is ITextChannel textChannel)
            {
                var logTextChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

                if (logTextChannel != null)
                {
                    CachedMessage message = new(new(cachedMessage.Id), await GetCachedMessageDataAsync(cachedMessage));

                    await logTextChannel.SendMessageAsync(embed: _messageLogEmbedFactory.CreateMessageDeleted(message, textChannel));
                }
            }
        }

        public async Task OnMessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, IMessageChannel channel)
        {
            if (channel is ITextChannel textChannel)
            {
                var logTextChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

                if (logTextChannel != null)
                {
                    List<CachedMessage> messages = new();
                    foreach (var cachedMessage in cachedMessages)
                    {
                        messages.Add(new(new(cachedMessage.Id), await GetCachedMessageDataAsync(cachedMessage)));
                    }

                    foreach (var embed in _messageLogEmbedFactory.CreateMessageBulkDeleted(messages, textChannel))
                    {
                        await logTextChannel.SendMessageAsync(embed: embed);
                    }
                }
            }
        }

        public async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> oldMessage, IMessage newMessage, IMessageChannel channel)
        {
            if (channel is ITextChannel textChannel)
            {
                if (!newMessage.Author.IsBot)
                {
                    var editedLogChannel = await _messageLogChannelFinder.FindEditedLogChannelAsync(textChannel.Guild);
                    if (editedLogChannel != null)
                    {
                        CachedMessage message = new(new(oldMessage.Id), await GetCachedMessageDataAsync(oldMessage));

                        await editedLogChannel.SendMessageAsync(embed: _messageLogEmbedFactory.CreateMessageEdited(message, newMessage, textChannel));

                        await CacheMessageAsync(newMessage);
                    }
                    else
                    {
                        var deletedLogChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);
                        if (deletedLogChannel != null)
                        {
                            await CacheMessageAsync(newMessage);
                        }
                    }
                }
                else
                {
                    var deletedLogChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);
                    if (deletedLogChannel != null)
                    {
                        await CacheMessageAsync(newMessage);
                    }
                }
            }
        }


        public async Task OnGuildUserMessageReceivedAsync(SocketTextChannel textChannel, SocketMessage message)
        {
            var deletedLogChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

            if (deletedLogChannel != null)
            {
                await CacheMessageAsync(message);
            }
            else
            {
                if (!message.Author.IsBot)
                {
                    var editedLogChannel = await _messageLogChannelFinder.FindEditedLogChannelAsync(textChannel.Guild);
                    if (editedLogChannel != null)
                    {
                        await CacheMessageAsync(message);
                    }
                }
            }
        }

        private async ValueTask CacheMessageAsync(IMessage newMessage)
        {
            await _cachedMessageRepository.SaveMessageAsync(
                new(newMessage.Id),
                new(
                    AuthorTag: $"{newMessage.Author.Username}#{newMessage.Author.Discriminator}",
                    AuthorId: newMessage.Author.Id.ToString(),
                    SystemMessageType: newMessage is ISystemMessage systemMessage ? systemMessage.Type : null,
                    Content: newMessage is IUserMessage userMessage ? userMessage.Content : null
                )
            );
        }
    }
}
