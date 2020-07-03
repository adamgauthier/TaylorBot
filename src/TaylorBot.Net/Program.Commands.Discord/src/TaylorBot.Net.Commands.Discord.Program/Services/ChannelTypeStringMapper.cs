using Discord;
using System;

namespace TaylorBot.Net.Commands.Discord.Program.Services
{
    public class ChannelTypeStringMapper
    {
        public string MapChannelToTypeString(IChannel channel)
        {
            return channel switch
            {
                ITextChannel _ => "Text",
                IVoiceChannel _ => "Voice",
                ICategoryChannel _ => "Category",
                IDMChannel _ => "DM",
                _ => throw new ArgumentOutOfRangeException(nameof(channel)),
            };
        }
    }
}
