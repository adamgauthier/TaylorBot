using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public class ChannelTypeStringMapper
{
    public string MapChannelToTypeString(ChannelType type) => type switch
    {
        ChannelType.Voice or ChannelType.Stage => "Voice",
        ChannelType.Text or ChannelType.News or ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread => "Text",
        ChannelType.Category => "Category",
        ChannelType.DM => "DM",
        ChannelType.Forum => "Forum",
        _ => "Unknown",
    };
}
