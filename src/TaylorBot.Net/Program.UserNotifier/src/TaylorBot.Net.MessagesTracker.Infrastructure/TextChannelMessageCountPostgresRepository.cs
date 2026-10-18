using Dapper;
using Discord;
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

        await TrackingBatchPersistence.PersistAsync(postgresConnectionFactory, redis,
            [MessageCountIncrementsHashKey], (connection, transaction, hashes) =>
        {
            var entries = hashes[0]
                .Select(entry =>
                {
                    var nameParts = entry.Name.ToString().Split(':');
                    return new
                    {
                        GuildId = nameParts[1],
                        ChannelId = nameParts[3],
                        Increment = (long)entry.Value,
                    };
                })
                .OrderBy(entry => entry.GuildId, StringComparer.Ordinal)
                .ThenBy(entry => entry.ChannelId, StringComparer.Ordinal)
                .ToArray();

            return connection.ExecuteAsync(
                    """
                    WITH updates AS (
                        SELECT * FROM unnest(@GuildIds::text[], @ChannelIds::text[], @Messages::bigint[])
                            AS u(guild_id, channel_id, messages)
                    ), locked AS MATERIALIZED (
                        SELECT c.guild_id, c.channel_id FROM guilds.text_channels c
                        JOIN updates u USING (guild_id, channel_id)
                        ORDER BY c.guild_id COLLATE "C", c.channel_id COLLATE "C"
                        FOR NO KEY UPDATE OF c
                    )
                    UPDATE guilds.text_channels c SET message_count = c.message_count + u.messages
                    FROM updates u JOIN locked l USING (guild_id, channel_id)
                    WHERE c.guild_id = l.guild_id AND c.channel_id = l.channel_id;
                    """,
                    new
                    {
                        GuildIds = entries.Select(entry => entry.GuildId).ToArray(),
                        ChannelIds = entries.Select(entry => entry.ChannelId).ToArray(),
                        Messages = entries.Select(entry => entry.Increment).ToArray(),
                    }, transaction);
        });
    }
}
