using Discord;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Core.Strings;

public static class UserFormattingExtensions
{
    public static string FormatTagAndMention(this DiscordUser user)
    {
        return $"{user.Handle} ({user.Mention})";
    }

    public static string FormatTagAndMention(this IUser user) => new DiscordUser(user).FormatTagAndMention();

    public static string FormatTagAndMention(this Interaction.User user)
    {
        var handle = $"{user.username}{(user.discriminator is "0" or "0000" ? "" : $"#{user.discriminator}")}";
        var mention = MentionUtils.MentionUser(new SnowflakeId(user.id));
        return $"{handle} ({mention})";
    }

    public static string Handle(this IUser user) => new DiscordUser(user).Handle;
}
