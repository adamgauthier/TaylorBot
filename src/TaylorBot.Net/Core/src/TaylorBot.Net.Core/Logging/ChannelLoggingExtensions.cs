using Discord;

namespace TaylorBot.Net.Core.Logging
{
    public static class ChannelLoggingExtensions
    {
        public static string FormatLog(this IMessageChannel messageChannel)
        {
            switch (messageChannel)
            {
                case ITextChannel textChannel:
                    return textChannel.FormatLog();
                case IDMChannel dmChannel:
                    return dmChannel.FormatLog();
                default:
                    return $"{messageChannel.Name} ({messageChannel.Id})";
            }
        }

        public static string FormatLog(this IDMChannel dmChannel)
        {
            return $"DM with [{dmChannel.Recipient.Username} ({dmChannel.Recipient.Id})] ({dmChannel.Id})";
        }

        public static string FormatLog(this ITextChannel textChannel)
        {
            return ((IGuildChannel)textChannel).FormatLog();
        }

        public static string FormatLog(this IGuildChannel guildChannel)
        {
            return $"{guildChannel.Name} ({guildChannel.Id}) on {guildChannel.Guild.Name} ({guildChannel.GuildId})";
        }
    }
}
