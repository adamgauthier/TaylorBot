using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.User;

public record DiscordUser(SnowflakeId Id, string Username, string? Avatar, string Discriminator, bool IsBot, DiscordMemberInfo? MemberInfo)
{
    public DiscordUser(IUser user) : this(user.Id, user.Username, user.AvatarId, user.Discriminator, user.IsBot, MemberInfo: user is IGuildUser guildUser ? new(
        guildUser.GuildId, guildUser.JoinedAt, guildUser.RoleIds.Select(r => new SnowflakeId(r)).ToList(), guildUser.GuildPermissions, guildUser.GuildAvatarId) : null)
    { }

    public DiscordUser(IGuildUser user) : this(user.Id, user.Username, user.AvatarId, user.Discriminator, user.IsBot, new(
        user.GuildId, user.JoinedAt, user.RoleIds.Select(r => new SnowflakeId(r)).ToList(), user.GuildPermissions, user.GuildAvatarId))
    { }

    public string Mention => MentionUtils.MentionUser(Id);

    public string Handle =>
        $"{Username}{(Discriminator is "0" or "0000" ? "" : $"#{Discriminator}")}";

    public string FormatLog() =>
        $"{Username}#{Discriminator} ({Id}){(MemberInfo != null ? $" in guild {MemberInfo.GuildId}" : "")}";
}

public record DiscordMemberInfo(SnowflakeId GuildId, DateTimeOffset? JoinedAt, IReadOnlyList<SnowflakeId> Roles, GuildPermissions Permissions, string? GuildAvatar);

public record DiscordMember(DiscordUser User, DiscordMemberInfo Member);
