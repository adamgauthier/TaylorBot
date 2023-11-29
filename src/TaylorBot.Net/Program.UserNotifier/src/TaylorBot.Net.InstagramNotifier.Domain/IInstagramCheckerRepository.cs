using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.InstagramNotifier.Domain
{
    public interface IInstagramCheckerRepository
    {
        ValueTask<IReadOnlyCollection<InstagramChecker>> GetInstagramCheckersAsync();
        ValueTask UpdateLastPostAsync(InstagramChecker instagramChecker, InstagramPost instagramPost);
    }

    public class InstagramChecker
    {
        public SnowflakeId GuildId { get; }
        public SnowflakeId ChannelId { get; }
        public string InstagramUsername { get; }
        public string? LastPostCode { get; }
        public DateTimeOffset LastPostTakenAt { get; }

        public InstagramChecker(SnowflakeId guildId, SnowflakeId channelId, string instagramUsername, string? lastPostCode, DateTimeOffset lastPostTakenAt)
        {
            GuildId = guildId;
            ChannelId = channelId;
            InstagramUsername = instagramUsername;
            LastPostCode = lastPostCode;
            LastPostTakenAt = lastPostTakenAt;
        }

        public override string ToString()
        {
            return $"Instagram Checker for Guild {GuildId}, Channel {ChannelId}, Username {InstagramUsername}";
        }
    }
}
