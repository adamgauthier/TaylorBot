using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.InstagramNotifier.Domain;

public interface IInstagramCheckerRepository
{
    ValueTask<IReadOnlyCollection<InstagramChecker>> GetInstagramCheckersAsync();
    ValueTask UpdateLastPostAsync(InstagramChecker instagramChecker, InstagramPost instagramPost);
}

public class InstagramChecker(SnowflakeId guildId, SnowflakeId channelId, string instagramUsername, string? lastPostCode, DateTimeOffset lastPostTakenAt)
{
    public SnowflakeId GuildId { get; } = guildId;
    public SnowflakeId ChannelId { get; } = channelId;
    public string InstagramUsername { get; } = instagramUsername;
    public string? LastPostCode { get; } = lastPostCode;
    public DateTimeOffset LastPostTakenAt { get; } = lastPostTakenAt;

    public override string ToString()
    {
        return $"Instagram Checker for Guild {GuildId}, Channel {ChannelId}, Username {InstagramUsername}";
    }
}
