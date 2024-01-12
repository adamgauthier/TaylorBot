using Discord.WebSocket;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Client;

public static class GuildExtensions
{
    public static SocketTextChannel GetRequiredTextChannel(this SocketGuild socketGuild, SnowflakeId id)
    {
        return socketGuild.GetTextChannel(id.Id) ?? throw new ArgumentException($"Could not resolve Text Channel ID {id}.");
    }
}
