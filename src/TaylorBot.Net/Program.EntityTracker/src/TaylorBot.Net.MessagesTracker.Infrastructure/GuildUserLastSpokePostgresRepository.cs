using Dapper;
using Discord;
using StackExchange.Redis;
using System.Globalization;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.MessagesTracker.Infrastructure;

public class GuildUserLastSpokePostgresRepository(PostgresConnectionFactory postgresConnectionFactory, ConnectionMultiplexer connectionMultiplexer) : IGuildUserLastSpokeRepository
{
    private const string LastSpokeUpdatesHashKey = "guild-user-last-spoke-updates";

    public async ValueTask QueueUpdateLastSpokeAsync(IGuildUser guildUser, DateTimeOffset lastSpokeAt)
    {
        var redis = connectionMultiplexer.GetDatabase();

        HashEntry entry = new($"guild:{guildUser.GuildId}:channel:{guildUser.Id}", lastSpokeAt.ToString("o"));

        await redis.HashSetAsync(LastSpokeUpdatesHashKey, new[] { entry });
    }

    public async ValueTask PersistQueuedLastSpokeUpdatesAsync()
    {
        var redis = connectionMultiplexer.GetDatabase();

        var tempKey = $"{LastSpokeUpdatesHashKey}:{Guid.NewGuid():N}";
        var renameSucceeded = await TryRenameKeyAsync(redis, LastSpokeUpdatesHashKey, tempKey);

        if (renameSucceeded)
        {
            var entries = await redis.HashGetAllAsync(tempKey);

            foreach (var entry in entries)
            {
                var nameParts = entry.Name.ToString().Split(':');
                var guildId = nameParts[1];
                var userId = nameParts[3];
                var lastSpokeAt = DateTimeOffset.ParseExact($"{entry.Value}", "o", CultureInfo.InvariantCulture);

                await using var connection = postgresConnectionFactory.CreateConnection();

                await connection.ExecuteAsync(
                    @"UPDATE guilds.guild_members SET last_spoke_at = @LastSpokeAt
                        WHERE guild_id = @GuildId AND user_id = @UserId;",
                    new
                    {
                        LastSpokeAt = lastSpokeAt.ToUniversalTime(),
                        GuildId = guildId,
                        UserId = userId
                    }
                );
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
