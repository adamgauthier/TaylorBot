using Dapper;
using Npgsql;
using StackExchange.Redis;
using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task<TrackingBatchOverlap> OpposingTrackingBatchesAsync(ScenarioMessage first, ScenarioMessage second, bool pauseMinutes)
    {
        await OrderTrackingHashAsync("guild-user-last-spoke-updates",
            [$"guild:{first.Guild.Id}:user:{first.User.Id}", $"guild:{second.Guild.Id}:user:{second.User.Id}"]);
        await OrderTrackingHashAsync("member-message-count-increments",
            [$"guild:{second.Guild.Id}:user:{second.User.Id}", $"guild:{first.Guild.Id}:user:{first.User.Id}"]);
        await OrderTrackingHashAsync("member-word-count-increments",
            [$"guild:{second.Guild.Id}:user:{second.User.Id}", $"guild:{first.Guild.Id}:user:{first.User.Id}"]);

        return await TrackingBatchOverlap.CreateAsync(database, first, pauseMinutes);
    }

    public Task QueueOlderLastSpokeAsync(ScenarioMessage message) =>
        database.Redis.HashSetAsync("guild-user-last-spoke-updates",
            $"guild:{message.Guild.Id}:user:{message.User.Id}", message.Timestamp.AddDays(-1).ToString("o"));

    public Task QueueMixedLastSpokeFieldNamesAsync(ScenarioMessage message, bool legacyIsLatest) =>
        database.Redis.HashSetAsync("guild-user-last-spoke-updates",
        [
            new HashEntry($"guild:{message.Guild.Id}:channel:{message.User.Id}",
                (legacyIsLatest ? message.Timestamp : message.Timestamp.AddDays(-1)).ToString("o")),
            new HashEntry($"guild:{message.Guild.Id}:user:{message.User.Id}",
                (legacyIsLatest ? message.Timestamp.AddDays(-1) : message.Timestamp).ToString("o")),
        ]);

    public async Task QueueLegacyLastSpokeAsync(ScenarioMessage message)
    {
        var transaction = database.Redis.CreateTransaction();
        _ = transaction.HashDeleteAsync("guild-user-last-spoke-updates", $"guild:{message.Guild.Id}:user:{message.User.Id}");
        _ = transaction.HashSetAsync("guild-user-last-spoke-updates",
            $"guild:{message.Guild.Id}:channel:{message.User.Id}", message.Timestamp.ToString("o"));
        if (!await transaction.ExecuteAsync())
        {
            throw new InvalidOperationException("Could not arrange a legacy last-spoke field.");
        }
    }

    private async Task OrderTrackingHashAsync(string key, string[] order)
    {
        var entries = (await database.Redis.HashGetAllAsync(key)).ToDictionary(entry => entry.Name.ToString());
        if (entries.Count != order.Length || order.Any(name => !entries.ContainsKey(name)))
        {
            throw new InvalidOperationException($"Expected exactly the arranged members in {key}.");
        }

        await database.Redis.KeyDeleteAsync(key);
        await database.Redis.HashSetAsync(key, [.. order.Select(name => entries[name])]);
        var actual = await database.Redis.HashGetAllAsync(key);
        if (!actual.Select(entry => entry.Name.ToString()).SequenceEqual(order))
        {
            throw new InvalidOperationException($"Redis did not retain the arranged batch order in {key}.");
        }
    }
}

public sealed class TrackingBatchOverlap : IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private bool _released;
    private bool _disposed;

    private TrackingBatchOverlap(NpgsqlConnection connection) => _connection = connection;

    internal static async Task<TrackingBatchOverlap> CreateAsync(ScenarioDatabase database, ScenarioMessage first, bool pauseMinutes)
    {
        var connection = database.CreateConnection();
        try
        {
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            await connection.ExecuteAsync("""
                CREATE FUNCTION public.integration_pause_tracking() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    PERFORM pg_advisory_xact_lock(7341, TG_ARGV[0]::integer);
                    RETURN NEW;
                END;
                $$;
                SELECT pg_advisory_lock(7341, 1), pg_advisory_lock(7341, 2);
                """);

            await connection.ExecuteAsync($"""
                -- Both writers pause on the same first row after acquiring its lock.
                CREATE TRIGGER integration_pause_last_spoke AFTER UPDATE OF {(pauseMinutes ? "minute_count" : "last_spoke_at")} ON guilds.guild_members
                FOR EACH ROW WHEN (NEW.guild_id = '{ulong.Parse(first.Guild.Id)}' AND NEW.user_id = '{ulong.Parse(first.User.Id)}')
                EXECUTE FUNCTION public.integration_pause_tracking('1');

                CREATE TRIGGER integration_pause_member_messages AFTER UPDATE OF message_count ON guilds.guild_members
                FOR EACH ROW WHEN (NEW.guild_id = '{ulong.Parse(first.Guild.Id)}' AND NEW.user_id = '{ulong.Parse(first.User.Id)}')
                EXECUTE FUNCTION public.integration_pause_tracking('2');
                """);

            return new(connection);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task ReleaseWhenBatchesContendAsync()
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(10));

        try
        {
            while (true)
            {
                var contending = await _connection.QuerySingleAsync<bool>(new CommandDefinition("""
                    SELECT EXISTS (
                        SELECT FROM pg_locks gate
                        JOIN pg_stat_activity writer ON gate.pid = ANY(pg_blocking_pids(writer.pid))
                        WHERE gate.locktype = 'advisory'
                            AND gate.database = (SELECT oid FROM pg_database WHERE datname = current_database())
                            AND gate.classid = 7341 AND gate.objid IN (1, 2) AND gate.objsubid = 2 AND NOT gate.granted
                            AND writer.datname = current_database()
                    );
                    """, cancellationToken: deadline.Token));
                if (contending)
                {
                    return;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(10), deadline.Token);
            }
        }
        finally
        {
            await ReleaseAsync();
        }
    }

    private async Task ReleaseAsync()
    {
        if (!_released)
        {
            await _connection.ExecuteAsync("SELECT pg_advisory_unlock_all();");
            _released = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        try
        {
            await ReleaseAsync();
            await _connection.ExecuteAsync("""
                DROP TRIGGER integration_pause_last_spoke ON guilds.guild_members;
                DROP TRIGGER integration_pause_member_messages ON guilds.guild_members;
                DROP FUNCTION public.integration_pause_tracking();
                """);
        }
        finally
        {
            await _connection.DisposeAsync();
        }
    }
}
