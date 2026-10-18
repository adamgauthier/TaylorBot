using Dapper;
using Npgsql;
using StackExchange.Redis;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.MessagesTracker.Infrastructure;

internal static class TrackingBatchPersistence
{
    internal const int MaxRows = 500;

    public static async Task PersistAsync(PostgresConnectionFactory connectionFactory, IDatabase redis, RedisKey[] sources,
        Func<NpgsqlConnection, NpgsqlTransaction, HashEntry[][], Task> write)
    {
        var queue = sources[0].ToString();
        var remaining = await TrackingQueueRecovery.QueuedCountAsync(redis, sources);
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        while (true)
        {
            await using var transaction = await connection.BeginTransactionAsync();
            // This lock also serializes Redis publication and acknowledgement across workers.
            var checkpoint = await connection.QuerySingleAsync<long>(
                "SELECT sequence FROM tracking.queue_checkpoints WHERE queue_name = @queue FOR UPDATE;",
                new { queue }, transaction);
            var batch = await TrackingQueueRecovery.ReadAsync(redis, sources);
            if (batch != null && batch.Sequence <= checkpoint)
            {
                await TrackingQueueRecovery.AcknowledgeAsync(redis, sources, batch.Sequence);
                remaining -= batch.Entries[0].Length;
                batch = null;
            }

            if (batch == null)
            {
                if (remaining <= 0)
                {
                    await transaction.CommitAsync();
                    return;
                }

                await TrackingQueueRecovery.PublishAsync(redis, sources, checked(checkpoint + 1));
                batch = await TrackingQueueRecovery.ReadAsync(redis, sources);
                if (batch == null)
                {
                    await transaction.CommitAsync();
                    return;
                }
            }

            if (batch.Sequence != checked(checkpoint + 1))
            {
                throw new InvalidOperationException($"Tracking queue {queue} has a sequence gap after {checkpoint}.");
            }

            await write(connection, transaction, batch.Entries);
            await connection.ExecuteAsync(
                "UPDATE tracking.queue_checkpoints SET sequence = @Sequence WHERE queue_name = @queue;",
                new { batch.Sequence, queue }, transaction);
            await transaction.CommitAsync();
            // A failed commit response or Redis acknowledgement leaves this immutable batch replayable.
        }
    }
}
