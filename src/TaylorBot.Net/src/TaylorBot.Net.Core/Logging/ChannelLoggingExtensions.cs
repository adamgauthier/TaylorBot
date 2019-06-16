using Discord;

namespace TaylorBot.Net.Core.Logging
{
    public static class ChannelLoggingExtensions
    {
        public static string FormatLog(this IGuildChannel textChannel)
        {
            return $"{textChannel.Name} ({textChannel.Id}) in {textChannel.Guild.Name} ({textChannel.GuildId})";
        }
    }
}
