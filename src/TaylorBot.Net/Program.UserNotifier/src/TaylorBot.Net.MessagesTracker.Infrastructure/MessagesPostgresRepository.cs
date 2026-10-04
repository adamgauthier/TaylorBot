using Dapper;
using Discord;
using Npgsql;
using StackExchange.Redis;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.MessagesTracker.Infrastructure;

public class MessagesPostgresRepository(PostgresConnectionFactory postgresConnectionFactory, ConnectionMultiplexer connectionMultiplexer) : IMessageRepository
{
    private const string MessageCountIncrementsHashKey = "member-message-count-increments";
    private const string WordCountIncrementsHashKey = "member-word-count-increments";

    public async ValueTask QueueAddMessagesAndWordsAsync(IGuildUser guildUser, long messageCountToAdd, long wordCountToAdd)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var transaction = redis.CreateTransaction();
        var hashKey = $"guild:{guildUser.GuildId}:user:{guildUser.Id}";

        _ = transaction.HashIncrementAsync(MessageCountIncrementsHashKey, hashKey, messageCountToAdd);
        _ = transaction.HashIncrementAsync(WordCountIncrementsHashKey, hashKey, wordCountToAdd);

        var wasCommitted = await transaction.ExecuteAsync();
        if (!wasCommitted)
            throw new InvalidOperationException($"Transaction was not committed for message/word increment of {hashKey}.");
    }

    public async ValueTask PersistQueuedMessagesAndWordsAsync()
    {
        var redis = connectionMultiplexer.GetDatabase();

        var messageTempKey = $"{MessageCountIncrementsHashKey}:{Guid.NewGuid():N}";
        var wordTempKey = $"{WordCountIncrementsHashKey}:{Guid.NewGuid():N}";
        var renamed = (bool)await redis.ScriptEvaluateAsync(
            """
            local messages = redis.call('EXISTS', KEYS[1])
            local words = redis.call('EXISTS', KEYS[2])
            if messages == 0 and words == 0 then return 0 end
            if messages ~= words then return redis.error_reply('Message and word tracking queues are incomplete') end
            redis.call('RENAME', KEYS[1], KEYS[3])
            redis.call('RENAME', KEYS[2], KEYS[4])
            return 1
            """,
            [MessageCountIncrementsHashKey, WordCountIncrementsHashKey, messageTempKey, wordTempKey]);

        if (renamed)
        {
            var messageEntries = await redis.HashGetAllAsync(messageTempKey);
            var wordEntries = await redis.HashGetAllAsync(wordTempKey);

            var grouped = messageEntries.GroupJoin(wordEntries,
                messageEntry => messageEntry.Name,
                wordEntry => wordEntry.Name,
                (messageEntry, wordEntry) =>
                {
                    var nameParts = messageEntry.Name.ToString().Split(':');
                    return new
                    {
                        GuildId = nameParts[1],
                        UserId = nameParts[3],
                        MessageIncrement = (long)messageEntry.Value,
                        WordIncrement = (long)wordEntry.Single().Value,
                    };
                }
            ).ToList();

            try
            {
                await using var connection = postgresConnectionFactory.CreateConnection();
                await connection.OpenAsync();
                await using var postgresTransaction = await connection.BeginTransactionAsync();

                foreach (var entry in grouped)
                {
                    await connection.ExecuteAsync(
                        """
                        UPDATE guilds.guild_members SET
                            message_count = message_count + @MessageCountToAdd,
                            word_count = word_count + @WordCountToAdd
                        WHERE guild_id = @GuildId AND user_id = @UserId;
                        """,
                        new
                        {
                            MessageCountToAdd = entry.MessageIncrement,
                            WordCountToAdd = entry.WordIncrement,
                            GuildId = entry.GuildId,
                            UserId = entry.UserId,
                        },
                        transaction: postgresTransaction
                    );
                }
                await postgresTransaction.CommitAsync();
            }
            catch (PostgresException failure)
            {
                await TrackingQueueRecovery.RestoreCountsAsync(redis, failure,
                    (MessageCountIncrementsHashKey, messageTempKey, messageEntries),
                    (WordCountIncrementsHashKey, wordTempKey, wordEntries));
                throw;
            }

            var transaction = redis.CreateTransaction();

            _ = transaction.KeyDeleteAsync(messageTempKey);
            _ = transaction.KeyDeleteAsync(wordTempKey);

            var wasCommitted = await transaction.ExecuteAsync();
            if (!wasCommitted)
                throw new InvalidOperationException($"Transaction was not committed for deleting keys {messageTempKey},{wordTempKey}.");
        }
    }
}
