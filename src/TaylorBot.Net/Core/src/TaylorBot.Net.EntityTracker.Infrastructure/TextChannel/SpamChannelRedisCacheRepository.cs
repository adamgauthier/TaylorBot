using StackExchange.Redis;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel;

public class SpamChannelRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, SpamChannelPostgresRepository spamChannelPostgresRepository) : ISpamChannelRepository
{
    private static string GetKey(GuildTextChannel channel) =>
        $"spam-channel:guild:{channel.GuildId}:channel:{channel.Id}";

    public async ValueTask<bool> InsertOrGetIsSpamChannelAsync(GuildTextChannel channel)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
        var cachedSpamChannel = await redis.StringGetAsync(key);

        if (!cachedSpamChannel.HasValue)
        {
            var isSpam = await spamChannelPostgresRepository.InsertOrGetIsSpamChannelAsync(channel);
            await redis.StringSetAsync(
                key,
                isSpam,
                TimeSpan.FromHours(1)
            );
            return isSpam;
        }

        return (bool)cachedSpamChannel;
    }

    public async ValueTask AddSpamChannelAsync(GuildTextChannel channel)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
        var isSpam = true;

        await spamChannelPostgresRepository.AddSpamChannelAsync(channel);

        await redis.StringSetAsync(
            key,
            isSpam,
            TimeSpan.FromHours(1)
        );
    }

    public async ValueTask RemoveSpamChannelAsync(GuildTextChannel channel)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
        var isSpam = false;

        await spamChannelPostgresRepository.RemoveSpamChannelAsync(channel);

        await redis.StringSetAsync(
            key,
            isSpam,
            TimeSpan.FromHours(1)
        );
    }
}
