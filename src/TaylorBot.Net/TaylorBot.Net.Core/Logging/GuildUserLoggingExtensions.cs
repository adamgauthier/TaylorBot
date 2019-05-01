using Discord;

namespace TaylorBot.Net.Core.Logging
{
    public static class GuildUserLoggingExtensions
    {
        public static string FormatLog(this IGuild guild)
        {
            return $"{guild.Name} ({guild.Id})";
        }
    }
}
