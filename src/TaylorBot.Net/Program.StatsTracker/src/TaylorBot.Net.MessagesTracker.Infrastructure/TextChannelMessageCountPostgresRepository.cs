using Dapper;
using Discord;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.MessagesTracker.Infrastructure
{
    public class TextChannelMessageCountPostgresRepository : ITextChannelMessageCountRepository
    {
        private const string IncrementKeysSetKey = "channel-message-count-increments-keys";

        private readonly PostgresConnectionFactory _postgresConnectionFactory;
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public TextChannelMessageCountPostgresRepository(PostgresConnectionFactory postgresConnectionFactory, ConnectionMultiplexer connectionMultiplexer)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async ValueTask QueueIncrementMessageCountAsync(ITextChannel channel)
        {
            var key = $"channel-message-count-increment:guild:{channel.GuildId}:channel:{channel.Id}";

            var redis = _connectionMultiplexer.GetDatabase();
            var transation = redis.CreateTransaction();

            _ = transation.StringIncrementAsync(key, 1);
            _ = transation.SetAddAsync(IncrementKeysSetKey, key);

            await transation.ExecuteAsync();
        }

        public async ValueTask PersistQueuedMessageCountIncrementsAsync()
        {
            var redis = _connectionMultiplexer.GetDatabase();

            var keys = await redis.SetPopAsync(IncrementKeysSetKey, count: 4294967295);

            foreach (var key in keys.Select(k => k.ToString()))
            {
                var incrementValue = await redis.StringGetSetAsync(key, 0);

                if (!incrementValue.IsNull)
                {
                    var increment = (long)incrementValue;

                    if (increment > 0)
                    {
                        var keyParts = key.Split(':');
                        var guildId = keyParts[2];
                        var channelId = keyParts[4];

                        using var connection = _postgresConnectionFactory.CreateConnection();

                        await connection.ExecuteAsync(
                            @"UPDATE guilds.text_channels
                            SET message_count = message_count + @MessageCountToAdd
                            WHERE guild_id = @GuildId AND channel_id = @ChannelId;",
                            new
                            {
                                MessageCountToAdd = increment,
                                GuildId = guildId,
                                ChannelId = channelId
                            }
                        );
                    }
                }
            }
        }
    }
}
