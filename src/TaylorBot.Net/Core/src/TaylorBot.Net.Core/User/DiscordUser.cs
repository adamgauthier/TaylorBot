using Discord;
using System.Diagnostics.CodeAnalysis;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.User;

public record DiscordUser(SnowflakeId Id, string Username, string? Avatar, string Discriminator, bool IsBot, DiscordMemberInfo? MemberInfo)
{
    public DiscordUser(IUser user) : this(user.Id, user.Username, user.AvatarId, user.Discriminator, user.IsBot, MemberInfo: user is IGuildUser guildUser ? new(guildUser) : null)
    { }

    public DiscordUser(IGuildUser user) : this(user.Id, user.Username, user.AvatarId, user.Discriminator, user.IsBot, new(user))
    { }

    public string Mention => MentionUtils.MentionUser(Id);

    public bool TryGetMember([NotNullWhen(true)] out DiscordMember? member)
    {
        if (MemberInfo != null)
        {
            member = new(this, MemberInfo);
            return true;
        }
        else
        {
            member = null;
            return false;
        }
    }

    public string Handle =>
        $"{Username}{(Discriminator is "0" or "0000" ? "" : $"#{Discriminator}")}";

    public string FormatLog() =>
        $"{Username}#{Discriminator} ({Id}){(MemberInfo != null ? $" in guild {MemberInfo.GuildId}" : "")}";
}

public record DiscordMemberInfo(SnowflakeId GuildId, DateTimeOffset? JoinedAt, IReadOnlyList<SnowflakeId> Roles, GuildPermissions Permissions, string? GuildAvatar)
{
    public DiscordMemberInfo(IGuildUser user) : this(
        user.GuildId, user.JoinedAt, [.. user.RoleIds.Select(r => new SnowflakeId(r))], user.GuildPermissions, user.GuildAvatarId)
    { }
}

public record DiscordMember(DiscordUser User, DiscordMemberInfo Member)
{
    public DiscordMember(IGuildUser user) : this(new(user), new(user))
    { }
}
