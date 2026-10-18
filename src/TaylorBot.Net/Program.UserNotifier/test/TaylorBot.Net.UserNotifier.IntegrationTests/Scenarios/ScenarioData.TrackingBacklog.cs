using Dapper;
using StackExchange.Redis;
using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed record TrackingBacklog(ScenarioMessage Message, IReadOnlyList<ScenarioUser> Users, IReadOnlyList<string> ChannelIds);

public sealed partial class ScenarioData
{
    public async Task<TrackingBacklog> ExpandTrackingBacklogAsync(ScenarioMessage message, int count)
    {
        var users = Enumerable.Range(1, count - 1).Select(_ => UntrackedUser()).Prepend(message.User).ToArray();
        var channels = Enumerable.Range(1, count - 1).Select(_ => $"{Interlocked.Increment(ref _id)}").Prepend(message.Guild.ChannelId).ToArray();
        var newUserIds = users.Skip(1).Select(user => user.Id).ToArray();
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO users.users (user_id, username, is_bot)
            SELECT id, 'Backlogged user', false FROM unnest(@UserIds::text[]) AS id;
            INSERT INTO guilds.guild_members (guild_id, user_id)
            SELECT @GuildId, id FROM unnest(@UserIds::text[]) AS id;
            INSERT INTO guilds.text_channels (guild_id, channel_id)
            SELECT @GuildId, id FROM unnest(@ChannelIds::text[]) AS id;
            """, new { UserIds = newUserIds, ChannelIds = channels.Skip(1).ToArray(), GuildId = message.Guild.Id });

        // Expand real Gateway-produced entries without dispatching thousands of unrelated entity events.
        var timestamp = await database.Redis.HashGetAsync("guild-user-last-spoke-updates",
            $"guild:{message.Guild.Id}:user:{message.User.Id}");
        await database.Redis.HashSetAsync("guild-user-last-spoke-updates",
            [.. users.Skip(1).Select(user => new HashEntry($"guild:{message.Guild.Id}:user:{user.Id}", timestamp))]);
        await database.Redis.HashSetAsync("member-message-count-increments",
            [.. users.Skip(1).Select(user => new HashEntry($"guild:{message.Guild.Id}:user:{user.Id}", 1))]);
        await database.Redis.HashSetAsync("member-word-count-increments",
            [.. users.Skip(1).Select(user => new HashEntry($"guild:{message.Guild.Id}:user:{user.Id}", 2))]);
        await database.Redis.HashSetAsync("channel-message-count-increments",
            [.. channels.Skip(1).Select(channel => new HashEntry($"guild:{message.Guild.Id}:channel:{channel}", 1))]);

        return new(message, users, channels);
    }

    public async Task<IAsyncDisposable> RejectLaterTrackingChunkAsync(UserNotifierJob job)
    {
        var (queue, table, column) = job switch
        {
            UserNotifierJob.LastSpoke => ("guild-user-last-spoke-updates", "guilds.guild_members", "last_spoke_at"),
            UserNotifierJob.MemberMessages => ("member-message-count-increments", "guilds.guild_members", "message_count"),
            UserNotifierJob.ChannelMessages => ("channel-message-count-increments", "guilds.text_channels", "message_count"),
            _ => throw new ArgumentOutOfRangeException(nameof(job)),
        };
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync($"""
            CREATE FUNCTION public.integration_reject_later_chunk() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
                IF (SELECT sequence FROM tracking.queue_checkpoints WHERE queue_name = '{queue}') > 0 THEN
                    RAISE EXCEPTION 'Reject later tracking chunk';
                END IF;
                RETURN NEW;
            END;
            $$;
            CREATE TRIGGER integration_reject_later_chunk AFTER UPDATE OF {column} ON {table}
            FOR EACH ROW EXECUTE FUNCTION public.integration_reject_later_chunk();
            """);
        return new ReversibleDatabaseChange(database, $"""
            DROP TRIGGER integration_reject_later_chunk ON {table};
            DROP FUNCTION public.integration_reject_later_chunk();
            """);
    }
}

public sealed record TrackingBacklogProgress(int Persisted, int Queued, int Invalid);

public sealed partial class ScenarioState
{
    public async Task<TrackingBacklogProgress> TrackingBacklogProgressAsync(UserNotifierJob job, TrackingBacklog backlog)
    {
        await using var connection = database.CreateConnection();
        var guildId = backlog.Message.Guild.Id;
        var values = job switch
        {
            UserNotifierJob.LastSpoke => (await connection.QueryAsync<long>(
                """
                SELECT CASE WHEN last_spoke_at IS NULL THEN 0
                    WHEN last_spoke_at = @Timestamp THEN 1 ELSE -1 END
                FROM guilds.guild_members WHERE guild_id = @guildId AND user_id = ANY(@Ids);
                """, new { guildId, Ids = backlog.Users.Select(user => user.Id).ToArray(), backlog.Message.Timestamp })).ToArray(),
            UserNotifierJob.MemberMessages => (await connection.QueryAsync<long>(
                """
                SELECT CASE WHEN word_count = 2 * message_count THEN message_count ELSE -1 END
                FROM guilds.guild_members WHERE guild_id = @guildId AND user_id = ANY(@Ids);
                """, new { guildId, Ids = backlog.Users.Select(user => user.Id).ToArray() })).ToArray(),
            UserNotifierJob.ChannelMessages => (await connection.QueryAsync<long>(
                "SELECT message_count FROM guilds.text_channels WHERE guild_id = @guildId AND channel_id = ANY(@Ids);",
                new { guildId, Ids = backlog.ChannelIds.ToArray() })).ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(job)),
        };
        values.Should().HaveCount(backlog.Users.Count);

        var key = job switch
        {
            UserNotifierJob.LastSpoke => "guild-user-last-spoke-updates",
            UserNotifierJob.MemberMessages => "member-message-count-increments",
            UserNotifierJob.ChannelMessages => "channel-message-count-increments",
            _ => throw new ArgumentOutOfRangeException(nameof(job)),
        };
        var queued = (await database.Redis.HashGetAllAsync(key))
            .Concat(await database.Redis.HashGetAllAsync($"{key}:pending")).ToArray();
        if (job == UserNotifierJob.MemberMessages)
        {
            var words = (await database.Redis.HashGetAllAsync("member-word-count-increments"))
                .Concat(await database.Redis.HashGetAllAsync("member-word-count-increments:pending")).ToArray();
            words.Select(entry => entry.Name).Should().BeEquivalentTo(queued.Select(entry => entry.Name));
            words.Where(entry => (long)entry.Value != 2).Should().BeEmpty();
        }
        if (job != UserNotifierJob.LastSpoke)
        {
            queued.Where(entry => (long)entry.Value != 1).Should().BeEmpty();
        }

        return new(values.Count(value => value == 1), queued.Length, values.Count(value => value is not (0 or 1)));
    }
}
