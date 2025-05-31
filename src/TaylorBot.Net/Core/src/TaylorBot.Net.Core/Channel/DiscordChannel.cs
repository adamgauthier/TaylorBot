using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Channel;

public record DiscordChannel(SnowflakeId Id, ChannelType Type)
{
    public string Mention => MentionUtils.MentionChannel(Id);
}
