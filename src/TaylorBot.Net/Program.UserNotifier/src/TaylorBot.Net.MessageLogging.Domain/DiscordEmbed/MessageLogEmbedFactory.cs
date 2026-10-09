using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.MessageLogging.Domain.Options;

namespace TaylorBot.Net.MessageLogging.Domain.DiscordEmbed;

public class MessageLogEmbedFactory(IOptionsMonitor<MessageDeletedLoggingOptions> optionsMonitor)
{
    private EmbedBuilder CreateBaseMessageDeleted(CachedMessage cachedMessage, ITextChannel channel)
    {
        var builder = new EmbedBuilder()
            .AddField("Channel", channel.Mention, inline: true);

        if (cachedMessage.Data != null)
        {
            switch (cachedMessage.Data)
            {
                case DiscordNetCachedMessageData discordNet:
                    var message = discordNet.Message;

                    if (message.Author.Id != 0)
                    {
                        var avatarUrl = message.Author.GetAvatarUrlOrDefault();
                        builder.WithAuthor($"{message.Author.Handle()} ({message.Author.Id})", avatarUrl, avatarUrl);
                    }

                    builder.AddField("Sent", message.Timestamp.FormatRelative(), inline: true);

                    if (message.EditedTimestamp.HasValue)
                    {
                        builder.AddField("Edited", message.EditedTimestamp.Value.FormatRelative(), inline: true);
                    }

                    if (message.Activity != null)
                    {
                        builder.AddField("Activity", message.Activity.Type.ToString(), inline: true);
                    }

                    if (message.Embeds.Count != 0)
                    {
                        builder.AddField("Embed Count", message.Embeds.Count, inline: true);
                    }

                    if (message.Reference != null)
                    {
                        if (message.Reference.MessageId.IsSpecified)
                        {
                            if (message.Reference.ChannelId == channel.Id)
                            {
                                builder.AddField("Replying To", $"{message.Reference.MessageId.Value}".LinkToMessage($"{channel.Id}", $"{channel.GuildId}"), inline: true);
                            }
                            else
                            {
                                builder.AddField("Published Announcement From", $"{MentionUtils.MentionChannel(message.Reference.ChannelId)} in server `{message.Reference.GuildId.Value}`");
                            }
                        }
                        else if (message.Reference.GuildId.IsSpecified)
                        {
                            builder.AddField("Referenced Channel", $"{MentionUtils.MentionChannel(message.Reference.ChannelId)} in server `{message.Reference.GuildId.Value}`");
                        }
                    }

                    if (message.Attachments.Count > 0)
                    {
                        builder.AddField("Attachments", string.Join(" ", message.Attachments.Select(a => a.ProxyUrl)).Truncate(EmbedFieldBuilder.MaxFieldValueLength));

                        var previewAttachment = message.Attachments.FirstOrDefault(a => a.ContentType?.StartsWith("image", StringComparison.InvariantCulture) == true)
                            ?? message.Attachments.First();

                        builder.WithImageUrl(previewAttachment.ProxyUrl);
                    }

                    switch (message)
                    {
                        case ISystemMessage systemMessage:
                            builder
                                .WithTitle("System Message Type")
                                .WithDescription(GetSystemMessageTypeString(systemMessage.Type));
                            break;

                        case IUserMessage userMessage:
                            if (!string.IsNullOrEmpty(userMessage.Content))
                            {
                                builder.WithTitle("Message Content").WithDescription(userMessage.Content.Truncate(EmbedBuilder.MaxDescriptionLength));
                            }
                            break;
                    }
                    break;

                case TaylorBotCachedMessageData taylorBot:
                    builder
                        .WithAuthor($"{taylorBot.AuthorTag} ({taylorBot.AuthorId})")
                        .AddField("Sent", SnowflakeUtils.FromSnowflake(cachedMessage.Id).FormatRelative(), inline: true);

                    if (taylorBot.EmbedCount is > 0)
                    {
                        builder.AddField("Embed Count", taylorBot.EmbedCount.Value, inline: true);
                    }

                    if (!string.IsNullOrWhiteSpace(taylorBot.ReplyingToId))
                    {
                        builder.AddField("Replying To", taylorBot.ReplyingToId.LinkToMessage($"{channel.Id}", $"{channel.GuildId}"), inline: true);
                    }

                    if (taylorBot.AttachmentUrls?.Count > 0)
                    {
                        builder
                            .AddField("Attachments", string.Join(" ", taylorBot.AttachmentUrls).Truncate(EmbedFieldBuilder.MaxFieldValueLength))
                            .WithImageUrl(taylorBot.AttachmentUrls[0]);
                    }

                    if (taylorBot.SystemMessageType.HasValue)
                    {
                        builder
                            .WithTitle("System Message Type")
                            .WithDescription(GetSystemMessageTypeString(taylorBot.SystemMessageType.Value));
                    }
                    else if (!string.IsNullOrEmpty(taylorBot.Content))
                    {
                        builder.WithTitle("Message Content").WithDescription(taylorBot.Content.Truncate(EmbedBuilder.MaxDescriptionLength));
                    }
                    break;
            }
        }
        else
        {
            builder.AddField("Sent", SnowflakeUtils.FromSnowflake(cachedMessage.Id).FormatRelative(), inline: true);
        }

        return builder;
    }

    public Embed CreateMessageDeleted(CachedMessage cachedMessage, ITextChannel channel)
    {
        var options = optionsMonitor.CurrentValue;

        return CreateBaseMessageDeleted(cachedMessage, channel)
            .WithColor(DiscordColor.FromHexString(options.MessageDeletedEmbedColorHex))
            .WithFooter($"Message deleted ({cachedMessage.Id})")
            .WithCurrentTimestamp()
            .Build();
    }

    public Embed CreateMessageEdited(CachedMessage cachedMessage, IMessage newMessage, ITextChannel channel, MessageUpdate update)
    {
        var options = optionsMonitor.CurrentValue;
        IReadOnlyList<MessageUpdateAction> actions =
            [.. update.Actions.Where(action => !update.ContentChanged || action != MessageUpdateAction.AttachmentsChanged)];
        var threadCreated = actions.Contains(MessageUpdateAction.ThreadCreated);
        var messageActions = string.Join(", ", actions.Where(action => action != MessageUpdateAction.ThreadCreated).Select(GetActionText));
        var footerText = threadCreated ? "Thread create from message" : $"Message {messageActions}";
        if (threadCreated && messageActions.Length > 0)
        {
            footerText += $", message {messageActions}";
        }
        var subtypeColor = actions[0] switch
        {
            MessageUpdateAction.Pinned => options.MessagePinnedEmbedColorHex,
            MessageUpdateAction.Unpinned => options.MessageUnpinnedEmbedColorHex,
            MessageUpdateAction.Published or MessageUpdateAction.PublishedSourceDeleted => options.MessagePublishedEmbedColorHex,
            MessageUpdateAction.EmbedsSuppressed => options.MessageEmbedsSuppressedEmbedColorHex,
            MessageUpdateAction.EmbedsRestored => options.MessageEmbedsRestoredEmbedColorHex,
            MessageUpdateAction.AttachmentsChanged => options.MessageAttachmentsChangedEmbedColorHex,
            MessageUpdateAction.ThreadCreated => options.MessageThreadCreatedEmbedColorHex,
            _ => null,
        };

        var builder = new EmbedBuilder()
            .WithColor(DiscordColor.FromHexString(subtypeColor ?? options.MessageEditedEmbedColorHex))
            .WithFooter($"{footerText} ({cachedMessage.Id})");

        if (actions.Count == 1 && actions[0] == MessageUpdateAction.Edited && newMessage.EditedTimestamp.HasValue)
        {
            builder.WithTimestamp(newMessage.EditedTimestamp.Value);
        }
        else
        {
            builder.WithCurrentTimestamp();
        }

        var author = cachedMessage.Data is DiscordNetCachedMessageData discordNet && discordNet.Message.Author.Id != 0
            ? discordNet.Message.Author
            : newMessage.Author;
        if (author.Id != 0)
        {
            var avatarUrl = author.GetAvatarUrlOrDefault();
            builder.WithAuthor($"{author.Handle()} ({author.Id})", avatarUrl, avatarUrl);
        }
        else if (update.Before != null)
        {
            builder.WithAuthor($"{update.Before.AuthorTag} ({update.Before.AuthorId})");
        }

        if (update.ContentChanged)
        {
            builder
                .WithTitle("Message Content Before Edit")
                .WithDescription(DisplayContent(update.Before!.Content).Truncate(EmbedBuilder.MaxDescriptionLength))
                .AddField("Message Content After Edit", DisplayContent(newMessage.Content).Truncate(EmbedFieldBuilder.MaxFieldValueLength));
        }
        else if (update.Before?.Content == null)
        {
            builder
                .WithTitle("Unknown Message Content Before Edit")
                .WithDescription(
                    """
                    Unfortunately, I don't remember this message's content before the edit. 😕
                    This is likely because the message is too old.
                    """);

            builder.AddField("Message Content After Edit", DisplayContent(newMessage.Content).Truncate(EmbedFieldBuilder.MaxFieldValueLength));
        }

        if (update.AttachmentsChanged)
        {
            builder
                .AddField("Attachments Before", DisplayAttachments(update.Before!.Attachments!))
                .AddField("Attachments After", DisplayAttachments(update.After.Attachments!));
        }
        if (threadCreated)
        {
            builder.AddField("Thread", MentionUtils.MentionChannel(newMessage.Id), inline: true);
        }
        var embedCount = actions.Contains(MessageUpdateAction.EmbedsSuppressed)
            ? update.Before?.EmbedCount
            : actions.Contains(MessageUpdateAction.EmbedsRestored) ? update.After.EmbedCount : null;
        if (embedCount is int count)
        {
            builder.AddField("Embed Count", count, inline: true);
        }

        var sent = cachedMessage.Data is DiscordNetCachedMessageData cached
            ? cached.Message.Timestamp
            : SnowflakeUtils.FromSnowflake(cachedMessage.Id);
        builder
            .AddField(threadCreated ? "From" : "Link", $"{cachedMessage.Id}".LinkToMessage($"{channel.Id}", $"{channel.GuildId}"), inline: true)
            .AddField("Sent", sent.FormatRelative(), inline: true);

        if (builder.Description is { } description && builder.Length > EmbedBuilder.MaxEmbedLength)
        {
            builder.WithDescription(description.Truncate(EmbedBuilder.MaxEmbedLength - builder.Length + description.Length));
        }

        return builder.Build();
    }

    private static string GetActionText(MessageUpdateAction action) => action switch
    {
        MessageUpdateAction.Edited => "edited",
        MessageUpdateAction.Pinned => "pinned",
        MessageUpdateAction.Unpinned => "unpinned",
        MessageUpdateAction.Published => "published",
        MessageUpdateAction.EmbedsSuppressed => "embeds suppressed",
        MessageUpdateAction.EmbedsRestored => "embeds restored",
        MessageUpdateAction.AttachmentsChanged => "attachments changed",
        MessageUpdateAction.PublishedSourceDeleted => "published source deleted",
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown message update action."),
    };

    private static string DisplayContent(string? content) => string.IsNullOrEmpty(content) ? "*No text*" : content;

    private static string DisplayAttachments(IReadOnlyList<CachedAttachment> attachments) =>
        attachments.Count == 0 ? "*None*" : string.Join('\n', attachments.Select(a => $"{Format.Sanitize(a.Filename)}: {a.Url}"))
            .Truncate(EmbedFieldBuilder.MaxFieldValueLength);

    private static string GetSystemMessageTypeString(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.ChannelPinnedMessage => "A message was pinned",
            MessageType.GuildMemberJoin => "A user joined the server",
            _ => messageType.ToString()
        };
    }

    public IReadOnlyCollection<Embed> CreateMessageBulkDeleted(IReadOnlyCollection<CachedMessage> cachedMessages, ITextChannel channel)
    {
        var options = optionsMonitor.CurrentValue;
        var embedColor = DiscordColor.FromHexString(options.MessageBulkDeletedEmbedColorHex);

        var eventTime = DateTimeOffset.UtcNow;
        var bulkId = Guid.NewGuid();
        var footerText = $"{"message".ToQuantity(cachedMessages.Count)} deleted in bulk ({bulkId:N})";

        var areCached = cachedMessages.ToLookup(c => c.Data != null);
        var deletedCached = areCached[true].ToList();
        var deletedNotCached = areCached[false].ToList();

        var uncachedEmbeds = deletedNotCached.Chunk(40).Select(chunk => new EmbedBuilder()
            .WithTimestamp(eventTime)
            .WithColor(embedColor)
            .AddField("Channel", channel.Mention, inline: true)
            .WithTitle($"{"uncached message".ToQuantity(chunk.Length)} deleted (Id - Sent)")
            .WithDescription(string.Join('\n', chunk.Select(uncached =>
                $"`{uncached.Id}` - {SnowflakeUtils.FromSnowflake(uncached.Id).FormatRelative()}"
            )))
            .WithFooter($"{chunk.Length}/{footerText}")
            .Build()
        );

        var cachedEmbeds = deletedCached.Select(cachedMessage =>
            CreateBaseMessageDeleted(cachedMessage, channel)
                .WithColor(embedColor)
                .WithFooter($"Message deleted ({cachedMessage.Id}) - {footerText}")
                .WithTimestamp(eventTime)
                .Build()
        );

        return [.. uncachedEmbeds, .. cachedEmbeds];
    }

    public Embed CreateReactionRemoved(ulong messageId, ITextChannel channel, SocketReaction reaction)
    {
        var options = optionsMonitor.CurrentValue;

        var embed = new EmbedBuilder()
            .WithColor(DiscordColor.FromHexString(options.MessageReactionRemovedEmbedColorHex))
            .AddField("Emote", reaction.Emote, inline: true)
            .AddField("Message", $"{messageId}".LinkToMessage($"{channel.Id}", $"{channel.GuildId}"), inline: true)
            .WithFooter("Reaction removed")
            .WithCurrentTimestamp();

        if (reaction.User.IsSpecified)
        {
            embed.WithUserAsAuthor(reaction.User.Value);
        }
        else
        {
            embed.AddField("User", $"{MentionUtils.MentionUser(reaction.UserId)}", inline: true);
        }

        return embed.Build();
    }
}
