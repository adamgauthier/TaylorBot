using Discord;
using StackExchange.Redis;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel;

public class SpamChannelRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, SpamChannelPostgresRepository spamChannelPostgresRepository) : ISpamChannelRepository
{
    private static string GetKey(GuildTextChannel channel)
    {
        return $"spam-channel:guild:{channel.GuildId}:channel:{channel.Id}";
    }

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

    public async ValueTask AddSpamChannelAsync(ITextChannel channel)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(new(channel.Id, channel.GuildId));
        var isSpam = true;

        await spamChannelPostgresRepository.AddSpamChannelAsync(channel);

        await redis.StringSetAsync(
            key,
            isSpam,
            TimeSpan.FromHours(1)
        );
    }

    public async ValueTask RemoveSpamChannelAsync(ITextChannel channel)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(new(channel.Id, channel.GuildId));
        var isSpam = false;

        await spamChannelPostgresRepository.RemoveSpamChannelAsync(channel);

        await redis.StringSetAsync(
            key,
            isSpam,
            TimeSpan.FromHours(1)
        );
    }
}
