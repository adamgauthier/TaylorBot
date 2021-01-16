using Discord;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel
{
    public class SpamChannelRedisCacheRepository : ISpamChannelRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly SpamChannelPostgresRepository _spamChannelPostgresRepository;

        public SpamChannelRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, SpamChannelPostgresRepository spamChannelPostgresRepository)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _spamChannelPostgresRepository = spamChannelPostgresRepository;
        }

        public async ValueTask<bool> InsertOrGetIsSpamChannelAsync(ITextChannel channel)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = $"spam-channel:guild:{channel.GuildId}:channel:{channel.Id}";
            var cachedSpamChannel = await redis.StringGetAsync(key);

            if (!cachedSpamChannel.HasValue)
            {
                var isSpam = await _spamChannelPostgresRepository.InsertOrGetIsSpamChannelAsync(channel);
                await redis.StringSetAsync(
                    key,
                    isSpam,
                    TimeSpan.FromHours(1)
                );
                return isSpam;
            }

            return (bool)cachedSpamChannel;
        }
    }
}
