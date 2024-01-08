using Dapper;
using DontPanic.TumblrSharp.Client;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.TumblrNotifier.Domain;

namespace TaylorBot.Net.TumblrNotifier.Infrastructure;

public class TumblrCheckerRepository(PostgresConnectionFactory postgresConnectionFactory) : ITumblrCheckerRepository
{
    private class TumblrCheckerDto
    {
        public string guild_id { get; set; } = null!;
        public string channel_id { get; set; } = null!;
        public string tumblr_user { get; set; } = null!;
        public string? last_link { get; set; }
    }

    public async ValueTask<IReadOnlyCollection<TumblrChecker>> GetTumblrCheckersAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var checkers = await connection.QueryAsync<TumblrCheckerDto>(
            "SELECT guild_id, channel_id, tumblr_user, last_link FROM checkers.tumblr_checker;"
        );

        return checkers.Select(checker => new TumblrChecker(
            guildId: new SnowflakeId(checker.guild_id),
            channelId: new SnowflakeId(checker.channel_id),
            blogName: checker.tumblr_user,
            lastPostShortUrl: checker.last_link
        )).ToList();
    }

    public async ValueTask UpdateLastPostAsync(TumblrChecker tumblrChecker, BasePost tumblrPost)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"UPDATE checkers.tumblr_checker SET last_link = @LastLink
                WHERE tumblr_user = @TumblrUser AND guild_id = @GuildId AND channel_id = @ChannelId;",
            new
            {
                TumblrUser = tumblrChecker.BlogName,
                GuildId = tumblrChecker.GuildId.ToString(),
                ChannelId = tumblrChecker.ChannelId.ToString(),
                LastLink = tumblrPost.ShortUrl
            }
        );
    }
}
