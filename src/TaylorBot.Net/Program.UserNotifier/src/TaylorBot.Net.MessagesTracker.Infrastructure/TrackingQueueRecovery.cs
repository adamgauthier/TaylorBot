using Npgsql;
using StackExchange.Redis;

namespace TaylorBot.Net.MessagesTracker.Infrastructure;

internal static class TrackingQueueRecovery
{
    // Only server-confirmed rejection is replayable; a lost commit acknowledgement is ambiguous.
    public static Task RestoreCountsAsync(IDatabase redis, PostgresException failure,
        params (RedisKey Source, RedisKey Snapshot, HashEntry[] Entries)[] hashes) => RestoreAsync(failure, async () =>
    {
        var transaction = redis.CreateTransaction();
        foreach (var hash in hashes)
        {
            foreach (var entry in hash.Entries)
            {
                _ = transaction.HashIncrementAsync(hash.Source, entry.Name, (long)entry.Value);
            }
            _ = transaction.KeyDeleteAsync(hash.Snapshot);
        }
        if (!await transaction.ExecuteAsync())
        {
            throw new InvalidOperationException("Could not restore the failed tracking batch.");
        }
    });

    public static Task RestoreLastSpokeAsync(IDatabase redis, PostgresException failure, RedisKey source, RedisKey snapshot, HashEntry[] entries) =>
        RestoreAsync(failure, async () =>
        {
            // A newer queued update wins over the failed snapshot.
            await redis.ScriptEvaluateAsync(
                """
                for i = 1, #ARGV, 2 do
                    redis.call('HSETNX', KEYS[1], ARGV[i], ARGV[i + 1])
                end
                return redis.call('DEL', KEYS[2])
                """,
                [source, snapshot],
                [.. entries.SelectMany(entry => new[] { entry.Name, entry.Value })]);
        });

    private static async Task RestoreAsync(PostgresException failure, Func<Task> restore)
    {
        try
        {
            await restore();
        }
        catch (Exception restoreFailure)
        {
            throw new AggregateException("PostgreSQL rejected tracking updates and restoring their Redis queue also failed.", failure, restoreFailure);
        }
    }
}
