using Discord;
using StackExchange.Redis;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel;

public class SpamChannelRedisCacheRepository : ISpamChannelRepository
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private readonly SpamChannelPostgresRepository _spamChannelPostgresRepository;

    public SpamChannelRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, SpamChannelPostgresRepository spamChannelPostgresRepository)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _spamChannelPostgresRepository = spamChannelPostgresRepository;
    }

    private static string GetKey(ITextChannel channel)
    {
        return $"spam-channel:guild:{channel.GuildId}:channel:{channel.Id}";
    }

    public async ValueTask<bool> InsertOrGetIsSpamChannelAsync(ITextChannel channel)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
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

    public async ValueTask AddSpamChannelAsync(ITextChannel channel)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
        var isSpam = true;

        await _spamChannelPostgresRepository.AddSpamChannelAsync(channel);

        await redis.StringSetAsync(
            key,
            isSpam,
            TimeSpan.FromHours(1)
        );
    }

    public async ValueTask RemoveSpamChannelAsync(ITextChannel channel)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
        var isSpam = false;

        await _spamChannelPostgresRepository.RemoveSpamChannelAsync(channel);

        await redis.StringSetAsync(
            key,
            isSpam,
            TimeSpan.FromHours(1)
        );
    }
}
