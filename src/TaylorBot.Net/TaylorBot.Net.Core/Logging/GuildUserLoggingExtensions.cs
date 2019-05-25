using Discord;

namespace TaylorBot.Net.Core.Logging
{
    public static class GuildUserLoggingExtensions
    {
        public static string FormatLog(this IGuildUser member)
        {
            return $"{member.Username}#{member.Discriminator} ({member.Id}) in {member.Guild.Name} ({member.GuildId})";
        }
    }
}
