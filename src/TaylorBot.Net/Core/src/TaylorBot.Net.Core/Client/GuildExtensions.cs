using Discord.WebSocket;
using System;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Client
{
    public static class GuildExtensions
    {
        public static SocketTextChannel GetRequiredTextChannel(this SocketGuild socketGuild, SnowflakeId id)
        {
            var channel = socketGuild.GetTextChannel(id.Id);
            if (channel == null)
            {
                throw new ArgumentException($"Could not resolve Text Channel ID {id}.");
            }
            return channel;
        }
    }
}
