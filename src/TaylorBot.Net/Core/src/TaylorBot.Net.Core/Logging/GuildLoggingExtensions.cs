using Discord;

namespace TaylorBot.Net.Core.Logging;

public static class GuildLoggingExtensions
{
    public static string FormatLog(this IGuild guild)
    {
        return $"{guild.Name} ({guild.Id})";
    }
}
