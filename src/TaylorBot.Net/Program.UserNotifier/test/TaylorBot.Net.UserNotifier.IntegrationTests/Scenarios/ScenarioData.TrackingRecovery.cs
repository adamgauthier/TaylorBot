using Dapper;
using Npgsql;
using StackExchange.Redis;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessagesTracker.Infrastructure;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    private static RedisKey[] TrackingSources(UserNotifierJob job) => job switch
    {
        UserNotifierJob.LastSpoke => ["guild-user-last-spoke-updates"],
        UserNotifierJob.MemberMessages => ["member-message-count-increments", "member-word-count-increments"],
        UserNotifierJob.ChannelMessages => ["channel-message-count-increments"],
        _ => throw new ArgumentOutOfRangeException(nameof(job)),
    };

    public async Task<PendingTrackingReplay> RememberPendingTrackingAsync(UserNotifierJob job)
    {
        var sources = TrackingSources(job);
        var sequence = await database.Redis.StringGetAsync($"{sources[0]}:pending-sequence");
        sequence.IsNull.Should().BeFalse();
        var hashes = await Task.WhenAll(sources.Select(source => database.Redis.HashGetAllAsync($"{source}:pending")));
        hashes.Should().OnlyContain(hash => hash.Length > 0 && hash.Length <= 500);
        return new(database.Redis, sources, sequence, hashes);
    }

    public Task FlushTrackingFromNewWorkerAsync(UserNotifierJob job) =>
        FlushTrackingFromNewWorkerAsync(job, database.ConnectionString);

    public async Task FlushTrackingWithLostCommitResponseAsync(UserNotifierJob job)
    {
        await using CommitResponseDropper connection = new(database.ConnectionString);
        var flush = () => FlushTrackingFromNewWorkerAsync(job, connection.ConnectionString);
        await flush.Should().ThrowAsync<NpgsqlException>();
        connection.CommitObserved.Should().BeTrue("the failure must follow a real PostgreSQL commit");
    }

    private async Task FlushTrackingFromNewWorkerAsync(UserNotifierJob job, string connectionString)
    {
        await using var postgres = NpgsqlDataSource.Create(connectionString);
        await using var redis = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
        {
            EndPoints = { database.Redis.Multiplexer.GetEndPoints().Single() },
        });
        PostgresConnectionFactory factory = new(postgres);
        await (job switch
        {
            UserNotifierJob.LastSpoke => new GuildUserLastSpokePostgresRepository(factory, redis).PersistQueuedLastSpokeUpdatesAsync(),
            UserNotifierJob.MemberMessages => new MessagesPostgresRepository(factory, redis).PersistQueuedMessagesAndWordsAsync(),
            UserNotifierJob.ChannelMessages => new TextChannelMessageCountPostgresRepository(factory, redis).PersistQueuedMessageCountIncrementsAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(job)),
        });
    }

    public async Task<TrackingWorkerOverlap> PauseTrackingQueueAsync(UserNotifierJob job) =>
        await TrackingWorkerOverlap.CreateAsync(database, TrackingSources(job)[0].ToString());

    public async Task<IAsyncDisposable> SkipPendingTrackingSequenceAsync(UserNotifierJob job)
    {
        RedisKey key = $"{TrackingSources(job)[0]}:pending-sequence";
        var original = await database.Redis.StringGetAsync(key);
        original.IsNull.Should().BeFalse();
        await database.Redis.StringIncrementAsync(key, value: 2);
        return new TrackingSequenceChange(database.Redis, key, original);
    }

    private sealed class TrackingSequenceChange(IDatabase redis, RedisKey key, RedisValue original) : IAsyncDisposable
    {
        private bool _disposed;

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await redis.StringSetAsync(key, original);
                _disposed = true;
            }
        }
    }
}

public sealed class PendingTrackingReplay(IDatabase redis, RedisKey[] sources, RedisValue sequence, HashEntry[][] hashes)
{
    public async Task ReplayAsync()
    {
        var transaction = redis.CreateTransaction();
        var sequenceKey = $"{sources[0]}:pending-sequence";
        transaction.AddCondition(Condition.KeyNotExists(sequenceKey));
        for (var index = 0; index < sources.Length; index++)
        {
            RedisKey pending = $"{sources[index]}:pending";
            transaction.AddCondition(Condition.KeyNotExists(pending));
            _ = transaction.HashSetAsync(pending, hashes[index]);
        }
        _ = transaction.StringSetAsync(sequenceKey, sequence);
        if (!await transaction.ExecuteAsync())
        {
            throw new InvalidOperationException("Cannot replay a delayed batch over existing pending tracking.");
        }
    }
}

public sealed class TrackingWorkerOverlap : IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;
    private bool _released;

    private TrackingWorkerOverlap(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    internal static async Task<TrackingWorkerOverlap> CreateAsync(ScenarioDatabase database, string queue)
    {
        var connection = database.CreateConnection();
        try
        {
            await connection.OpenAsync();
            var transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(
                "SELECT sequence FROM tracking.queue_checkpoints WHERE queue_name = @queue FOR UPDATE;",
                new { queue }, transaction);
            return new(connection, transaction);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task ReleaseWhenWorkersWaitAsync()
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            while (true)
            {
                await _connection.ExecuteAsync(new CommandDefinition("SELECT pg_stat_clear_snapshot();",
                    transaction: _transaction, cancellationToken: deadline.Token));
                var waiting = await _connection.QuerySingleAsync<int>(new CommandDefinition("""
                    SELECT count(*) FROM pg_stat_activity
                    WHERE datname = current_database() AND pid <> pg_backend_pid()
                        AND wait_event_type = 'Lock' AND query LIKE '%tracking.queue_checkpoints%FOR UPDATE%';
                    """, transaction: _transaction, cancellationToken: deadline.Token));
                if (waiting >= 2)
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
            await _transaction.CommitAsync();
            _released = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await ReleaseAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
