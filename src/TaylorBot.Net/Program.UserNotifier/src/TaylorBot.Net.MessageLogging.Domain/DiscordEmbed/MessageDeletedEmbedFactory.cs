using Discord;
using Microsoft.Extensions.Options;
using System.Linq;
using TaylorBot.Net.Core.Colors;
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

        public Embed CreateMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, ITextChannel channel)
        {
            var options = _optionsMonitor.CurrentValue;

            var builder = new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithColor(DiscordColor.FromHexString(options.MessageDeletedEmbedColorHex))
                .WithFooter($"Message deleted ({cachedMessage.Id})")
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
                    builder.AddField("Attachements", string.Join(" | ", message.Attachments.Select(a => $"[{a.Filename}]({a.ProxyUrl})")));
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

            return builder.Build();
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
    }
}
