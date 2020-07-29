using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.MessageLogging.Domain.Options;

namespace TaylorBot.Net.MessageLogging.Domain.DiscordEmbed
{
    public class MessageDeletedEmbedFactory
    {
        private readonly IOptionsMonitor<MessageDeletedLoggingOptions> _optionsMonitor;

        public MessageDeletedEmbedFactory(IOptionsMonitor<MessageDeletedLoggingOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        private EmbedBuilder CreateBaseMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, ITextChannel channel)
        {
            var builder = new EmbedBuilder()
                .AddField("Channel", channel.Mention, inline: true);

            var message = cachedMessage.Value;

            if (message != null)
            {
                var avatarUrl = message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl();

                builder
                    .WithAuthor($"{message.Author.Username}#{message.Author.Discriminator} ({message.Author.Id})", avatarUrl, avatarUrl)
                    .AddField("Sent At", message.Timestamp.FormatShortUserLogDate(), inline: true);

                if (message.EditedTimestamp.HasValue)
                {
                    builder.AddField("Edited At", message.EditedTimestamp.Value.FormatShortUserLogDate(), inline: true);
                }

                if (message.Activity != null)
                {
                    builder.AddField("Activity", message.Activity.Type.ToString(), inline: true);
                }

                if (message.Embeds.Any())
                {
                    builder.AddField("Embed Count", message.Embeds.Count, inline: true);
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
                            builder.WithTitle("Message Content").WithDescription(userMessage.Content);
                        }
                        break;
                }
            }
            else
            {
                builder.AddField("Sent At", SnowflakeUtils.FromSnowflake(cachedMessage.Id).FormatShortUserLogDate(), inline: true);
            }

            return builder;
        }

        public Embed CreateMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, ITextChannel channel)
        {
            var options = _optionsMonitor.CurrentValue;

            return CreateBaseMessageDeleted(cachedMessage, channel)
                .WithColor(DiscordColor.FromHexString(options.MessageDeletedEmbedColorHex))
                .WithFooter($"Message deleted ({cachedMessage.Id})")
                .WithCurrentTimestamp()
                .Build();
        }

        private string GetSystemMessageTypeString(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.ChannelPinnedMessage => "A message was pinned",
                MessageType.GuildMemberJoin => "A user joined the server",
                _ => messageType.ToString()
            };
        }

        public IReadOnlyCollection<Embed> CreateMessageBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, ITextChannel channel)
        {
            var options = _optionsMonitor.CurrentValue;
            var embedColor = DiscordColor.FromHexString(options.MessageBulkDeletedEmbedColorHex);

            var eventTime = DateTimeOffset.UtcNow;
            var bulkId = Guid.NewGuid();
            var footerText = $"{"message".ToQuantity(cachedMessages.Count)} deleted in bulk ({bulkId:N})";

            var areCached = cachedMessages.ToLookup(c => c.HasValue);
            var deletedCached = areCached[true].ToList();
            var deletedNotCached = areCached[false].ToList();

            var uncachedEmbeds = Split(deletedNotCached, 40).Select(chunk => new EmbedBuilder()
                .WithTimestamp(eventTime)
                .WithColor(embedColor)
                .AddField("Channel", channel.Mention, inline: true)
                .WithTitle($"{"uncached message".ToQuantity(chunk.Count)} deleted (Id - Sent At)")
                .WithDescription(string.Join('\n', chunk.Select(uncached =>
                    $"`{uncached.Id}` - `{SnowflakeUtils.FromSnowflake(uncached.Id).FormatShortUserLogDate()}`"
                )))
                .WithFooter($"{chunk.Count}/{footerText}")
                .Build()
            );

            var cachedEmbeds = deletedCached.Select(cachedMessage =>
                CreateBaseMessageDeleted(cachedMessage, channel)
                    .WithColor(embedColor)
                    .WithFooter($"1/{footerText}")
                    .WithTimestamp(eventTime)
                    .Build()
            );

            return uncachedEmbeds.Concat(cachedEmbeds).ToList();
        }

        private static IList<List<T>> Split<T>(IReadOnlyList<T> source, uint chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
