using Dapper;
using Discord;
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

        await TrackingBatchPersistence.PersistAsync(postgresConnectionFactory, redis,
            [MessageCountIncrementsHashKey, WordCountIncrementsHashKey], (connection, transaction, hashes) =>
        {
            var grouped = hashes[0].Zip(hashes[1], (messageEntry, wordEntry) =>
                {
                    var nameParts = messageEntry.Name.ToString().Split(':');
                    return new
                    {
                        GuildId = nameParts[1],
                        UserId = nameParts[3],
                        MessageIncrement = (long)messageEntry.Value,
                        WordIncrement = (long)wordEntry.Value,
                    };
                }
            ).OrderBy(entry => entry.GuildId, StringComparer.Ordinal)
                .ThenBy(entry => entry.UserId, StringComparer.Ordinal)
                .ToArray();

            return connection.ExecuteAsync(
                    """
                    WITH updates AS (
                        SELECT * FROM unnest(@GuildIds::text[], @UserIds::text[], @Messages::bigint[], @Words::bigint[])
                            AS u(guild_id, user_id, messages, words)
                    ), locked AS MATERIALIZED (
                        SELECT m.guild_id, m.user_id FROM guilds.guild_members m
                        JOIN updates u USING (guild_id, user_id)
                        ORDER BY m.guild_id COLLATE "C", m.user_id COLLATE "C"
                        FOR NO KEY UPDATE OF m
                    )
                    UPDATE guilds.guild_members m
                    SET message_count = m.message_count + u.messages, word_count = m.word_count + u.words
                    FROM updates u JOIN locked l USING (guild_id, user_id)
                    WHERE m.guild_id = l.guild_id AND m.user_id = l.user_id;
                    """,
                    new
                    {
                        GuildIds = grouped.Select(entry => entry.GuildId).ToArray(),
                        UserIds = grouped.Select(entry => entry.UserId).ToArray(),
                        Messages = grouped.Select(entry => entry.MessageIncrement).ToArray(),
                        Words = grouped.Select(entry => entry.WordIncrement).ToArray(),
                    }, transaction);
        });
    }
}
