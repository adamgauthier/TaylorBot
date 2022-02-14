using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.MessageLogging.Domain.Options;

namespace TaylorBot.Net.MessageLogging.Domain.DiscordEmbed
{
    public class MessageLogEmbedFactory
    {
        private readonly IOptionsMonitor<MessageDeletedLoggingOptions> _optionsMonitor;

        public MessageLogEmbedFactory(IOptionsMonitor<MessageDeletedLoggingOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

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
                            builder.WithAuthor($"{message.Author.Username}#{message.Author.Discriminator} ({message.Author.Id})", avatarUrl, avatarUrl);
                        }

                        builder.AddField("Sent", message.Timestamp.FormatShortUserLogDate(), inline: true);

                        if (message.EditedTimestamp.HasValue)
                        {
                            builder.AddField("Edited", message.EditedTimestamp.Value.FormatShortUserLogDate(), inline: true);
                        }

                        if (message.Activity != null)
                        {
                            builder.AddField("Activity", message.Activity.Type.ToString(), inline: true);
                        }

                        if (message.Embeds.Any())
                        {
                            builder.AddField("Embed Count", message.Embeds.Count, inline: true);
                        }

                        if (message.Reference != null)
                        {
                            if (message.Reference.MessageId.IsSpecified)
                            {
                                if (message.Reference.ChannelId == channel.Id)
                                {
                                    builder.AddField("Replying To", $"`{message.Reference.MessageId.Value}`", inline: true);
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

                        if (message.Attachments.Any())
                        {
                            builder.AddField("Attachments", string.Join(" | ", message.Attachments.Select(a => a.Filename.DiscordMdLink(a.ProxyUrl))));
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
                            .AddField("Sent", SnowflakeUtils.FromSnowflake(cachedMessage.Id.Id).FormatShortUserLogDate(), inline: true);

                        if (!string.IsNullOrWhiteSpace(taylorBot.ReplyingToId))
                        {
                            builder.AddField("Replying To", $"`{taylorBot.ReplyingToId}`", inline: true);
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
                builder.AddField("Sent", SnowflakeUtils.FromSnowflake(cachedMessage.Id.Id).FormatShortUserLogDate(), inline: true);
            }

            return builder;
        }

        public Embed CreateMessageDeleted(CachedMessage cachedMessage, ITextChannel channel)
        {
            var options = _optionsMonitor.CurrentValue;

            return CreateBaseMessageDeleted(cachedMessage, channel)
                .WithColor(DiscordColor.FromHexString(options.MessageDeletedEmbedColorHex))
                .WithFooter($"Message deleted ({cachedMessage.Id})")
                .WithCurrentTimestamp()
                .Build();
        }

        public Embed CreateMessageEdited(CachedMessage cachedMessage, IMessage newMessage, ITextChannel channel)
        {
            var options = _optionsMonitor.CurrentValue;

            var builder = new EmbedBuilder()
                .WithColor(DiscordColor.FromHexString(options.MessageEditedEmbedColorHex))
                .WithFooter($"Message edited ({cachedMessage.Id})");

            if (newMessage.EditedTimestamp.HasValue)
            {
                builder.WithTimestamp(newMessage.EditedTimestamp.Value);
            }
            else
            {
                builder.WithCurrentTimestamp();
            }

            if (cachedMessage.Data != null)
            {
                switch (cachedMessage.Data)
                {
                    case DiscordNetCachedMessageData discordNet:
                        var message = discordNet.Message;

                        var author = message.Author.Id != 0 ?
                            message.Author :
                            newMessage.Author.Id != 0 ? newMessage.Author : null;

                        if (author != null)
                        {
                            var avatarUrl = author.GetAvatarUrlOrDefault();
                            builder.WithAuthor($"{author.Username}#{author.Discriminator} ({author.Id})", avatarUrl, avatarUrl);
                        }

                        if (message is IUserMessage userMessage && !string.IsNullOrEmpty(userMessage.Content) &&
                            !string.IsNullOrEmpty(newMessage.Content) && userMessage.Content != newMessage.Content)
                        {
                            builder
                                .WithTitle("Message Content Before Edit")
                                .WithDescription(userMessage.Content.Truncate(EmbedBuilder.MaxDescriptionLength))
                                .AddField("Message Content After Edit", newMessage.Content.Truncate(EmbedFieldBuilder.MaxFieldValueLength));
                        }

                        builder
                            .AddField("Channel", channel.Mention, inline: true)
                            .AddField("Sent", message.Timestamp.FormatShortUserLogDate(), inline: true);
                        break;

                    case TaylorBotCachedMessageData taylorBot:
                        if (newMessage.Author.Id != 0)
                        {
                            var avatarUrl = newMessage.Author.GetAvatarUrlOrDefault();
                            builder.WithAuthor($"{newMessage.Author.Username}#{newMessage.Author.Discriminator} ({newMessage.Author.Id})", avatarUrl, avatarUrl);
                        }
                        else
                        {
                            builder.WithAuthor($"{taylorBot.AuthorTag} ({taylorBot.AuthorId})");
                        }

                        if (!string.IsNullOrEmpty(taylorBot.Content) && !string.IsNullOrEmpty(newMessage.Content) && taylorBot.Content != newMessage.Content)
                        {
                            builder
                                .WithTitle("Message Content Before Edit")
                                .WithDescription(taylorBot.Content.Truncate(EmbedBuilder.MaxDescriptionLength))
                                .AddField("Message Content After Edit", newMessage.Content.Truncate(EmbedFieldBuilder.MaxFieldValueLength));
                        }

                        builder
                            .AddField("Channel", channel.Mention, inline: true)
                            .AddField("Sent", SnowflakeUtils.FromSnowflake(cachedMessage.Id.Id).FormatShortUserLogDate(), inline: true);
                        break;
                }
            }
            else
            {
                if (newMessage.Author.Id != 0)
                {
                    var avatarUrl = newMessage.Author.GetAvatarUrlOrDefault();
                    builder.WithAuthor($"{newMessage.Author.Username}#{newMessage.Author.Discriminator} ({newMessage.Author.Id})", avatarUrl, avatarUrl);
                }

                builder
                    .WithTitle("Unknown Message Content Before Edit")
                    .WithDescription(string.Join('\n', new[] {
                        "Unfortunately, I don't remember this message's content before the edit. 😕",
                        "This is likely because the message is too old."
                    }));

                if (!string.IsNullOrEmpty(newMessage.Content))
                {
                    builder.AddField("Message Content After Edit", newMessage.Content.Truncate(EmbedFieldBuilder.MaxFieldValueLength));
                }

                builder
                    .AddField("Channel", channel.Mention, inline: true)
                    .AddField("Sent", SnowflakeUtils.FromSnowflake(cachedMessage.Id.Id).FormatShortUserLogDate(), inline: true);
            }

            return builder.Build();
        }

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
            var options = _optionsMonitor.CurrentValue;
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
                    $"`{uncached.Id}` - {SnowflakeUtils.FromSnowflake(uncached.Id.Id).FormatShortUserLogDate()}"
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

            return uncachedEmbeds.Concat(cachedEmbeds).ToList();
        }
    }
}
