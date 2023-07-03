using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.MessageLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MessageLogging.Domain;

public record CachedMessage(SnowflakeId Id, ICachedMessageData? Data);
public interface ICachedMessageData { }
public record DiscordNetCachedMessageData(IMessage Message) : ICachedMessageData;
public record TaylorBotCachedMessageData(string AuthorTag, string AuthorId, MessageType? SystemMessageType, string? Content, string? ReplyingToId, IReadOnlyList<string>? AttachmentUrls) : ICachedMessageData;

public interface ICachedMessageRepository
{
    ValueTask SaveMessageAsync(SnowflakeId messageId, TimeSpan expiry, TaylorBotCachedMessageData data);
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

    public async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachedMessage, IMessageChannel channel, SocketReaction reaction)
    {
        if (channel is ITextChannel textChannel)
        {
            var logTextChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

            if (logTextChannel != null)
            {
                await logTextChannel.Resolved.SendMessageAsync(embed: _messageLogEmbedFactory.CreateReactionRemoved(cachedMessage.Id, textChannel, reaction));
            }
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

                await logTextChannel.Resolved.SendMessageAsync(embed: _messageLogEmbedFactory.CreateMessageDeleted(message, textChannel));
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

                var embeds = _messageLogEmbedFactory.CreateMessageBulkDeleted(messages, textChannel);

                foreach (var chunk in embeds.Chunk(10))
                {
                    await logTextChannel.Resolved.SendMessageAsync(embeds: chunk);
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
                    var isEmbedOnlyEdit = oldMessage.HasValue && oldMessage.Value.Content == newMessage.Content && newMessage.Embeds.Count != oldMessage.Value.Embeds.Count;

                    if (!isEmbedOnlyEdit)
                    {
                        CachedMessage message = new(new(oldMessage.Id), await GetCachedMessageDataAsync(oldMessage));

                        await editedLogChannel.Resolved.SendMessageAsync(embed: _messageLogEmbedFactory.CreateMessageEdited(message, newMessage, textChannel));
                    }

                    await CacheMessageAsync(newMessage, editedLogChannel);
                }
                else
                {
                    var deletedLogChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);
                    if (deletedLogChannel != null)
                    {
                        await CacheMessageAsync(newMessage, deletedLogChannel);
                    }
                }
            }
            else
            {
                var deletedLogChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);
                if (deletedLogChannel != null)
                {
                    await CacheMessageAsync(newMessage, deletedLogChannel);
                }
            }
        }
    }


    public async Task OnGuildUserMessageReceivedAsync(SocketTextChannel textChannel, SocketMessage message)
    {
        var deletedLogChannel = await _messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

        if (deletedLogChannel != null)
        {
            await CacheMessageAsync(message, deletedLogChannel);
        }
        else
        {
            if (!message.Author.IsBot)
            {
                var editedLogChannel = await _messageLogChannelFinder.FindEditedLogChannelAsync(textChannel.Guild);
                if (editedLogChannel != null)
                {
                    await CacheMessageAsync(message, editedLogChannel);
                }
            }
        }
    }

    private async ValueTask CacheMessageAsync(IMessage newMessage, FoundChannel foundChannel)
    {
        var author = newMessage.Author;

        await _cachedMessageRepository.SaveMessageAsync(
            new(newMessage.Id),
            foundChannel.Channel.CacheExpiry ?? TimeSpan.FromMinutes(10),
            new(
                AuthorTag: $"{author.Username}{author.DiscrimSuffix()}",
                AuthorId: newMessage.Author.Id.ToString(),
                SystemMessageType: newMessage is ISystemMessage systemMessage ? systemMessage.Type : null,
                Content: newMessage is IUserMessage userMessage ? userMessage.Content : null,
                ReplyingToId: newMessage.Reference?.MessageId.IsSpecified == true && newMessage.Reference.ChannelId == newMessage.Channel.Id
                    ? $"{newMessage.Reference.MessageId.Value}"
                    : null,
                AttachmentUrls: newMessage.Attachments.Count > 0 ? newMessage.Attachments.Select(a => a.ProxyUrl).ToList() : null
            )
        );
    }
}
