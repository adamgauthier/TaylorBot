using Dapper;
using Discord;
using StackExchange.Redis;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.MessagesTracker.Infrastructure
{
    public class MessagesPostgresRepository : IMessageRepository
    {
        private const string MessageCountIncrementsHashKey = "member-message-count-increments";
        private const string WordCountIncrementsHashKey = "member-word-count-increments";

        private readonly PostgresConnectionFactory _postgresConnectionFactory;
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public MessagesPostgresRepository(PostgresConnectionFactory postgresConnectionFactory, ConnectionMultiplexer connectionMultiplexer)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async ValueTask QueueAddMessagesAndWordsAsync(IGuildUser guildUser, long messageCountToAdd, long wordCountToAdd)
        {
            var redis = _connectionMultiplexer.GetDatabase();
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
            var redis = _connectionMultiplexer.GetDatabase();

            var messageTempKey = $"{MessageCountIncrementsHashKey}:{Guid.NewGuid():N}";
            var messageRenameSucceeded = await TryRenameKeyAsync(redis, MessageCountIncrementsHashKey, messageTempKey);

            var wordTempKey = $"{WordCountIncrementsHashKey}:{Guid.NewGuid():N}";
            var wordRenameSucceeded = await TryRenameKeyAsync(redis, WordCountIncrementsHashKey, wordTempKey);

            if (messageRenameSucceeded && wordRenameSucceeded)
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

                foreach (var entry in grouped)
                {
                    await using var connection = _postgresConnectionFactory.CreateConnection();

                    await connection.ExecuteAsync(
                        @"UPDATE guilds.guild_members SET
                            message_count = message_count + @MessageCountToAdd,
                            word_count = word_count + @WordCountToAdd
                        WHERE guild_id = @GuildId AND user_id = @UserId;",
                        new
                        {
                            MessageCountToAdd = entry.MessageIncrement,
                            WordCountToAdd = entry.WordIncrement,
                            GuildId = entry.GuildId,
                            UserId = entry.UserId
                        }
                    );
                }

                var transaction = redis.CreateTransaction();

                _ = transaction.KeyDeleteAsync(messageTempKey);
                _ = transaction.KeyDeleteAsync(wordTempKey);

                var wasCommitted = await transaction.ExecuteAsync();
                if (!wasCommitted)
                    throw new InvalidOperationException($"Transaction was not committed for deleting keys {messageTempKey},{wordTempKey}.");
            }
        }

        private static async ValueTask<bool> TryRenameKeyAsync(IDatabase redis, RedisKey key, RedisKey newKey)
        {
            try
            {
                await redis.KeyRenameAsync(key, newKey);
                return true;
            }
            catch (RedisServerException e) when (e.Message == "ERR no such key")
            {
                return false;
            }
        }
    }
}
