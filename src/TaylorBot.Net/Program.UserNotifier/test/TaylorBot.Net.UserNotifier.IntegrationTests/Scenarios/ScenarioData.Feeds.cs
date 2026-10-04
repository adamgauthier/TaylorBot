using Dapper;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task FeedAsync(ScenarioGuild guild, UserNotifierJob job)
    {
        var (table, source, _) = FeedTables.For(job);
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            $"INSERT INTO {table} (guild_id, channel_id, {source}) VALUES (@Id, @ChannelId, 'integration');", new { guild.Id, guild.ChannelId });
    }

    public async Task NewerFeedCheckpointAsync(ScenarioGuild guild, UserNotifierJob job)
    {
        var (table, _, checkpoint) = FeedTables.For(job);
        var timestamp = job switch
        {
            UserNotifierJob.Reddit => "last_created",
            UserNotifierJob.Youtube => "last_published_at",
            _ => throw new ArgumentOutOfRangeException(nameof(job)),
        };

        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            $"UPDATE {table} SET {checkpoint} = 'newer-item', {timestamp} = CURRENT_TIMESTAMP + interval '1 day' WHERE guild_id = @Id AND channel_id = @ChannelId;",
            new { guild.Id, guild.ChannelId });
    }
}

internal static class FeedTables
{
    public static (string Table, string Source, string Checkpoint) For(UserNotifierJob job) => job switch
    {
        UserNotifierJob.Reddit => ("checkers.reddit_checker", "subreddit", "last_post_id"),
        UserNotifierJob.Youtube => ("checkers.youtube_checker", "playlist_id", "last_video_id"),
        UserNotifierJob.Tumblr => ("checkers.tumblr_checker", "tumblr_user", "last_link"),
        _ => throw new ArgumentOutOfRangeException(nameof(job)),
    };
}
