using Discord;
using Discord.WebSocket;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.MessageLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MessageLogging.Domain;

public record CachedMessage(SnowflakeId Id, ICachedMessageData? Data);
public interface ICachedMessageData { }
public record DiscordNetCachedMessageData(IMessage Message) : ICachedMessageData;
public record CachedAttachment(ulong Id, string Filename, string Url);
public record TaylorBotCachedMessageData(string AuthorTag, string AuthorId, MessageType? SystemMessageType, string? Content, string? ReplyingToId, IReadOnlyList<string>? AttachmentUrls,
    bool? IsPinned = null, bool? IsPublished = null, bool? EmbedsSuppressed = null, bool? HasThread = null,
    IReadOnlyList<CachedAttachment>? Attachments = null, bool? SourceMessageDeleted = null, int? EmbedCount = null) : ICachedMessageData
{
    public static TaylorBotCachedMessageData FromMessage(IMessage message) => new(
        AuthorTag: message.Author.Handle(),
        AuthorId: $"{message.Author.Id}",
        SystemMessageType: message is ISystemMessage systemMessage ? systemMessage.Type : null,
        Content: message is IUserMessage userMessage ? userMessage.Content : null,
        ReplyingToId: message.Reference?.MessageId.IsSpecified == true && message.Reference.ChannelId == message.Channel.Id
            ? $"{message.Reference.MessageId.Value}"
            : null,
        AttachmentUrls: message.Attachments.Count > 0 ? message.Attachments.Select(a => a.ProxyUrl).ToList() : null,
        IsPinned: message.IsPinned,
        IsPublished: HasFlag(message, MessageFlags.Crossposted),
        EmbedsSuppressed: HasFlag(message, MessageFlags.SuppressEmbeds),
        HasThread: HasFlag(message, MessageFlags.HasThread),
        Attachments: [.. message.Attachments.Select(a => new CachedAttachment(a.Id, a.Filename, a.ProxyUrl))],
        SourceMessageDeleted: HasFlag(message, MessageFlags.SourceMessageDeleted),
        EmbedCount: message.Embeds.Count
    );

    private static bool? HasFlag(IMessage message, MessageFlags flag) =>
        message.Flags.HasValue ? (message.Flags.Value & flag) == flag : null;
}

public interface ICachedMessageRepository
{
    ValueTask SaveMessageAsync(SnowflakeId messageId, TimeSpan expiry, TaylorBotCachedMessageData data);
    ValueTask<TaylorBotCachedMessageData?> GetMessageDataAsync(SnowflakeId messageId);
}

public class MessageLoggerService(MessageLogChannelFinder messageLogChannelFinder, MessageLogEmbedFactory messageLogEmbedFactory, ICachedMessageRepository cachedMessageRepository)
{
    private async ValueTask<ICachedMessageData?> GetCachedMessageDataAsync(Cacheable<IMessage, ulong> cachedMessage)
    {
        if (cachedMessage.Value != null)
        {
            return new DiscordNetCachedMessageData(cachedMessage.Value);
        }
        else
        {
            var messageData = await cachedMessageRepository.GetMessageDataAsync(new(cachedMessage.Id));
            return messageData;
        }
    }

    public async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachedMessage, IMessageChannel channel, SocketReaction reaction)
    {
        if (channel is ITextChannel textChannel)
        {
            var logTextChannel = await messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

            if (logTextChannel != null)
            {
                await logTextChannel.Resolved.SendMessageAsync(embed: messageLogEmbedFactory.CreateReactionRemoved(cachedMessage.Id, textChannel, reaction));
            }
        }
    }

    public async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, IMessageChannel channel)
    {
        if (channel is ITextChannel textChannel)
        {
            var logTextChannel = await messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

            if (logTextChannel != null)
            {
                CachedMessage message = new(new(cachedMessage.Id), await GetCachedMessageDataAsync(cachedMessage));

                await logTextChannel.Resolved.SendMessageAsync(embed: messageLogEmbedFactory.CreateMessageDeleted(message, textChannel));
            }
        }
    }

    public async Task OnMessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, IMessageChannel channel)
    {
        if (channel is ITextChannel textChannel)
        {
            var logTextChannel = await messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

            if (logTextChannel != null)
            {
                List<CachedMessage> messages = [];
                foreach (var cachedMessage in cachedMessages)
                {
                    messages.Add(new(new(cachedMessage.Id), await GetCachedMessageDataAsync(cachedMessage)));
                }

                var embeds = messageLogEmbedFactory.CreateMessageBulkDeleted(messages, textChannel);

                foreach (var chunk in BatchEmbeds(embeds))
                {
                    await logTextChannel.Resolved.SendMessageAsync(embeds: chunk);
                }
            }
        }
    }

    private static IEnumerable<Embed[]> BatchEmbeds(IEnumerable<Embed> embeds)
    {
        List<Embed> batch = [];
        var length = 0;
        foreach (var embed in embeds)
        {
            if (batch.Count > 0 && (batch.Count == 10 || length + embed.Length > EmbedBuilder.MaxEmbedLength))
            {
                yield return [.. batch];
                batch.Clear();
                length = 0;
            }
            batch.Add(embed);
            length += embed.Length;
        }
        if (batch.Count > 0)
        {
            yield return [.. batch];
        }
    }

    public async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> oldMessage, IMessage newMessage, IMessageChannel channel)
    {
        if (channel is ITextChannel textChannel)
        {
            if (!newMessage.Author.IsBot)
            {
                var editedLogChannel = await messageLogChannelFinder.FindEditedLogChannelAsync(textChannel.Guild);
                if (editedLogChannel != null)
                {
                    CachedMessage message = new(new(oldMessage.Id), await GetCachedMessageDataAsync(oldMessage));
                    var update = MessageUpdate.Compare(message.Data, newMessage);

                    if (update.Actions.Count > 0)
                    {
                        await editedLogChannel.Resolved.SendMessageAsync(embed: messageLogEmbedFactory.CreateMessageEdited(message, newMessage, textChannel, update));
                    }

                    await CacheMessageAsync(newMessage, editedLogChannel);
                }
                else
                {
                    var deletedLogChannel = await messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);
                    if (deletedLogChannel != null)
                    {
                        await CacheMessageAsync(newMessage, deletedLogChannel);
                    }
                }
            }
            else
            {
                var deletedLogChannel = await messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);
                if (deletedLogChannel != null)
                {
                    await CacheMessageAsync(newMessage, deletedLogChannel);
                }
            }
        }
    }


    public async Task OnGuildUserMessageReceivedAsync(SocketTextChannel textChannel, SocketMessage message)
    {
        var deletedLogChannel = await messageLogChannelFinder.FindDeletedLogChannelAsync(textChannel.Guild);

        if (deletedLogChannel != null)
        {
            await CacheMessageAsync(message, deletedLogChannel);
        }
        else
        {
            if (!message.Author.IsBot)
            {
                var editedLogChannel = await messageLogChannelFinder.FindEditedLogChannelAsync(textChannel.Guild);
                if (editedLogChannel != null)
                {
                    await CacheMessageAsync(message, editedLogChannel);
                }
            }
        }
    }

    private async ValueTask CacheMessageAsync(IMessage newMessage, FoundChannel foundChannel)
    {
        await cachedMessageRepository.SaveMessageAsync(
            new(newMessage.Id),
            foundChannel.Channel.CacheExpiry ?? TimeSpan.FromMinutes(10),
            TaylorBotCachedMessageData.FromMessage(newMessage)
        );
    }
}
