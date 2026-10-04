using Dapper;
using Discord;
using Npgsql;
using StackExchange.Redis;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.MessagesTracker.Infrastructure;

public class TextChannelMessageCountPostgresRepository(PostgresConnectionFactory postgresConnectionFactory, ConnectionMultiplexer connectionMultiplexer) : ITextChannelMessageCountRepository
{
    private const string MessageCountIncrementsHashKey = "channel-message-count-increments";

    public async ValueTask QueueIncrementMessageCountAsync(ITextChannel channel)
    {
        var redis = connectionMultiplexer.GetDatabase();

        await redis.HashIncrementAsync(MessageCountIncrementsHashKey, $"guild:{channel.GuildId}:channel:{channel.Id}");
    }

    public async ValueTask PersistQueuedMessageCountIncrementsAsync()
    {
        var redis = connectionMultiplexer.GetDatabase();

        var tempKey = $"{MessageCountIncrementsHashKey}:{Guid.NewGuid():N}";
        var renameSucceeded = await TryRenameKeyAsync(redis, MessageCountIncrementsHashKey, tempKey);

        if (renameSucceeded)
        {
            var entries = await redis.HashGetAllAsync(tempKey);

            try
            {
                await using var connection = postgresConnectionFactory.CreateConnection();
                await connection.OpenAsync();
                await using var transaction = await connection.BeginTransactionAsync();

                foreach (var entry in entries)
                {
                    var nameParts = entry.Name.ToString().Split(':');
                    var guildId = nameParts[1];
                    var channelId = nameParts[3];
                    var increment = (long)entry.Value;

                    await connection.ExecuteAsync(
                        """
                        UPDATE guilds.text_channels
                        SET message_count = message_count + @MessageCountToAdd
                        WHERE guild_id = @GuildId AND channel_id = @ChannelId;
                        """,
                        new
                        {
                            MessageCountToAdd = increment,
                            GuildId = guildId,
                            ChannelId = channelId,
                        },
                        transaction: transaction
                    );
                }
                await transaction.CommitAsync();
            }
            catch (PostgresException failure)
            {
                await TrackingQueueRecovery.RestoreCountsAsync(redis, failure, (MessageCountIncrementsHashKey, tempKey, entries));
                throw;
            }

            await redis.KeyDeleteAsync(tempKey);
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
