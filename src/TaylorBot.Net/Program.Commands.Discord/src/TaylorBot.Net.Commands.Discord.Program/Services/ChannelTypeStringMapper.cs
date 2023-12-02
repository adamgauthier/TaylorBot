using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public class ChannelTypeStringMapper
{
    public string MapChannelToTypeString(IChannel channel)
    {
        return channel switch
        {
            IVoiceChannel _ => "Voice",
            ITextChannel _ => "Text",
            ICategoryChannel _ => "Category",
            IDMChannel _ => "DM",
            _ => throw new ArgumentOutOfRangeException(nameof(channel)),
        };
    }
}
