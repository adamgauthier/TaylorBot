using StackExchange.Redis;
using System.Globalization;

namespace TaylorBot.Net.MessagesTracker.Infrastructure;

internal sealed record TrackingBatch(long Sequence, HashEntry[][] Entries);

internal static class TrackingQueueRecovery
{
    private static RedisKey SequenceKey(RedisKey source) => $"{source}:pending-sequence";
    private static RedisKey PendingKey(RedisKey source) => $"{source}:pending";

    public static async Task<long> QueuedCountAsync(IDatabase redis, RedisKey[] sources) =>
        (long)await redis.ScriptEvaluateAsync(
            """
            local count = redis.call('HLEN', KEYS[1])
            for h = 2, #KEYS - 1 do
                if redis.call('HLEN', KEYS[h]) ~= count then
                    return redis.error_reply('Tracking queues are incomplete')
                end
            end
            return count + redis.call('HLEN', KEYS[#KEYS])
            """,
            [.. sources, PendingKey(sources[0])]);

    public static async Task<TrackingBatch?> ReadAsync(IDatabase redis, RedisKey[] sources)
    {
        var sequence = await redis.StringGetAsync(SequenceKey(sources[0]));
        var entries = await Task.WhenAll(sources.Select(source => redis.HashGetAllAsync(PendingKey(source))));
        if (sequence.IsNull)
        {
            if (entries.Any(hash => hash.Length != 0))
            {
                throw new InvalidOperationException("Tracking payload has no durable batch sequence.");
            }
            return null;
        }

        var ordered = entries.Select(hash => hash.OrderBy(entry => entry.Name.ToString(), StringComparer.Ordinal).ToArray()).ToArray();
        if (ordered[0].Length is 0 or > TrackingBatchPersistence.MaxRows ||
            ordered.Any(hash => !hash.Select(entry => entry.Name).SequenceEqual(ordered[0].Select(entry => entry.Name))))
        {
            throw new InvalidOperationException("The pending tracking batch is incomplete or exceeds its row bound.");
        }

        var number = long.Parse(sequence.ToString(), CultureInfo.InvariantCulture);
        if (number <= 0)
        {
            throw new InvalidOperationException("The pending tracking sequence must be positive.");
        }
        return new(number, ordered);
    }

    public static async Task PublishAsync(IDatabase redis, RedisKey[] sources, long sequence)
    {
        HashSet<RedisValue> fields = [];
        await foreach (var entry in redis.HashScanAsync(sources[0], pageSize: TrackingBatchPersistence.MaxRows))
        {
            fields.Add(entry.Name);
            if (fields.Count == TrackingBatchPersistence.MaxRows)
            {
                break;
            }
        }
        await redis.ScriptEvaluateAsync(
            """
            if redis.call('EXISTS', KEYS[1]) ~= 0 then
                return redis.error_reply('Tracking batch is already pending')
            end
            local count = (#KEYS - 1) / 2
            local values = {}
            for h = 1, count do
                if redis.call('EXISTS', KEYS[1 + count + h]) ~= 0 then
                    return redis.error_reply('Tracking payload has no sequence')
                end
                if redis.call('HLEN', KEYS[1 + h]) ~= redis.call('HLEN', KEYS[2]) then
                    return redis.error_reply('Tracking queues are incomplete')
                end
                values[h] = {}
                for f = 2, #ARGV do
                    local value = redis.call('HGET', KEYS[1 + h], ARGV[f])
                    if not value then return redis.error_reply('Tracking field is missing') end
                    values[h][f] = value
                end
            end
            if #ARGV == 1 then return 0 end
            for h = 1, count do
                for f = 2, #ARGV do
                    redis.call('HSET', KEYS[1 + count + h], ARGV[f], values[h][f])
                    redis.call('HDEL', KEYS[1 + h], ARGV[f])
                end
            end
            redis.call('SET', KEYS[1], ARGV[1])
            return 1
            """,
            [SequenceKey(sources[0]), .. sources, .. sources.Select(PendingKey)],
            [sequence.ToString(CultureInfo.InvariantCulture), .. fields]);
    }

    public static async Task AcknowledgeAsync(IDatabase redis, RedisKey[] sources, long sequence)
    {
        await redis.ScriptEvaluateAsync(
            """
            if redis.call('GET', KEYS[1]) ~= ARGV[1] then
                return redis.error_reply('Tracking acknowledgement sequence changed')
            end
            return redis.call('DEL', unpack(KEYS))
            """,
            [SequenceKey(sources[0]), .. sources.Select(PendingKey)],
            [sequence.ToString(CultureInfo.InvariantCulture)]);
    }
}
