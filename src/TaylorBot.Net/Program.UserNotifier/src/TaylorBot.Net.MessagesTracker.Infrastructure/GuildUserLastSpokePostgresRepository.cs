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

        HashEntry entry = new($"guild:{guildUser.GuildId}:user:{guildUser.Id}", lastSpokeAt.ToString("o"));

        await redis.HashSetAsync(LastSpokeUpdatesHashKey, [entry]);
    }

    public async ValueTask PersistQueuedLastSpokeUpdatesAsync()
    {
        var redis = connectionMultiplexer.GetDatabase();

        await TrackingBatchPersistence.PersistAsync(postgresConnectionFactory, redis,
            [LastSpokeUpdatesHashKey], (connection, transaction, hashes) =>
        {
            var entries = hashes[0]
                .Select(entry =>
                {
                    var nameParts = entry.Name.ToString().Split(':');
                    return new
                    {
                        GuildId = nameParts[1],
                        UserId = nameParts[3],
                        LastSpokeAt = DateTimeOffset.ParseExact($"{entry.Value}", "o", CultureInfo.InvariantCulture).ToUniversalTime(),
                    };
                })
                .OrderBy(entry => entry.GuildId, StringComparer.Ordinal)
                .ThenBy(entry => entry.UserId, StringComparer.Ordinal)
                .ToArray();

            return connection.ExecuteAsync(
                    """
                    WITH updates AS (
                        SELECT guild_id, user_id, MAX(last_spoke_at) AS last_spoke_at
                        FROM unnest(@GuildIds::text[], @UserIds::text[], @LastSpokeAt::timestamptz[])
                            AS u(guild_id, user_id, last_spoke_at)
                        GROUP BY guild_id, user_id
                    ), locked AS MATERIALIZED (
                        SELECT m.guild_id, m.user_id FROM guilds.guild_members m
                        JOIN updates u USING (guild_id, user_id)
                        ORDER BY m.guild_id COLLATE "C", m.user_id COLLATE "C"
                        FOR NO KEY UPDATE OF m
                    )
                    UPDATE guilds.guild_members m
                    SET last_spoke_at = GREATEST(m.last_spoke_at, u.last_spoke_at)
                    FROM updates u JOIN locked l USING (guild_id, user_id)
                    WHERE m.guild_id = l.guild_id AND m.user_id = l.user_id;
                    """,
                    new
                    {
                        GuildIds = entries.Select(entry => entry.GuildId).ToArray(),
                        UserIds = entries.Select(entry => entry.UserId).ToArray(),
                        LastSpokeAt = entries.Select(entry => entry.LastSpokeAt).ToArray(),
                    }, transaction);
        });
    }
}
