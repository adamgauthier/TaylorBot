using TaylorBot.Net.UserNotifier.LoadTests;

namespace TaylorBot.Net.UserNotifier.LoadController;

internal sealed class Payloads(LoadProfile profile)
{
    public const string BotId = "100000000000000001";
    public static string GuildId(int index) => $"{(24_000_000_000UL + (ulong)index) << 22}";
    public static string ChannelId(int guild, int channel) => $"{300_000_000_000_000_000UL + (ulong)guild * 1000 + (ulong)channel}";
    public string UserId(int guild, int member) => member == 0 ? BotId : $"{200_000_000_000_000_000UL + (ulong)(guild % profile.UserPools) * 1_000_000 + (ulong)member}";
    public static string RoleId(int guild, int role) => role == 0 ? GuildId(guild) : $"{400_000_000_000_000_000UL + (ulong)guild * 1000 + (ulong)role}";
    public int MemberCount(int guild) => Math.Max(profile.CachedMembersPerGuild, (guild % 20) switch
    {
        0 => 50_000,
        < 4 => 5000,
        < 10 => 500,
        _ => 50,
    });
    public static object User(string id, bool bot = false) => new
    {
        id,
        username = bot ? "SyntheticBot" : $"Synthetic{id}",
        discriminator = "0",
        avatar = (string?)null,
        bot,
        public_flags = 0,
    };
    public object Member(int guild, int member) => new
    {
        user = User(UserId(guild, member), bot: member == 0),
        roles = member == 0 || profile.RolesPerGuild == 1 ? Array.Empty<string>() : new[] { RoleId(guild, role: 1) },
        joined_at = "2026-01-01T00:00:00Z",
        nick = (string?)null,
        deaf = false,
        mute = false,
        flags = 0,
    };
    public static object Channel(int guild, int channel, string? name = null) => new
    {
        id = ChannelId(guild, channel),
        guild_id = GuildId(guild),
        type = 0,
        name = name ?? $"synthetic-{channel}",
        position = channel,
        permission_overwrites = Array.Empty<object>(),
    };
    public static object Role(int guild, int role, string? name = null) => new
    {
        id = RoleId(guild, role),
        name = name ?? (role == 0 ? "@everyone" : $"synthetic-role-{role}"),
        color = 0,
        hoist = false,
        position = role,
        permissions = "68608",
        managed = false,
        mentionable = false,
        flags = 0,
    };
    public object Guild(int index) => new
    {
        id = GuildId(index),
        unavailable = false,
        name = $"Synthetic guild {index}",
        owner_id = UserId(index, member: 1),
        icon = (string?)null,
        afk_channel_id = (string?)null,
        afk_timeout = 300,
        verification_level = 0,
        default_message_notifications = 0,
        explicit_content_filter = 0,
        mfa_level = 0,
        premium_tier = 0,
        premium_subscription_count = 0,
        preferred_locale = "en-US",
        nsfw_level = 0,
        features = Array.Empty<string>(),
        emojis = Array.Empty<object>(),
        stickers = Array.Empty<object>(),
        roles = Enumerable.Range(0, profile.RolesPerGuild).Select(role => Role(index, role)).ToArray(),
        channels = Enumerable.Range(0, profile.ChannelsPerGuild).Select(channel => Channel(index, channel)).ToArray(),
        members = Enumerable.Range(0, profile.CachedMembersPerGuild).Select(member => Member(index, member)).ToArray(),
        member_count = MemberCount(index),
        large = MemberCount(index) > 250,
        joined_at = "2026-01-01T00:00:00Z",
        presences = Array.Empty<object>(),
        voice_states = Array.Empty<object>(),
        threads = Array.Empty<object>(),
        stage_instances = Array.Empty<object>(),
        guild_scheduled_events = Array.Empty<object>(),
    };
    public object Message(int guild, int channel, long id, string? content = null, bool edited = false) => new
    {
        id = $"{id}",
        channel_id = ChannelId(guild, channel),
        guild_id = GuildId(guild),
        author = User(UserId(guild, member: 1)),
        member = Member(guild, member: 1),
        content = content ?? new('x', profile.MessageCharacters),
        timestamp = DateTimeOffset.UtcNow.ToString("O"),
        edited_timestamp = edited ? DateTimeOffset.UtcNow.ToString("O") : null,
        tts = false,
        mention_everyone = false,
        mentions = Array.Empty<object>(),
        mention_roles = Array.Empty<string>(),
        attachments = Array.Empty<object>(),
        embeds = Array.Empty<object>(),
        pinned = false,
        type = 0,
        flags = 0,
    };
}
