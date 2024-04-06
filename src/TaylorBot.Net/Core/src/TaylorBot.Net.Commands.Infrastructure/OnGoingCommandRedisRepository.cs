using StackExchange.Redis;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Infrastructure;

public class OnGoingCommandRedisRepository(ConnectionMultiplexer connectionMultiplexer) : IOngoingCommandRepository
{
    private string GetKey(DiscordUser user, string pool) => $"ongoing-commands:user:{user.Id}{pool}";

    public async ValueTask AddOngoingCommandAsync(DiscordUser user, string pool)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(user, pool);
        await redis.StringSetAsync(key, "1", expiry: TimeSpan.FromSeconds(10));
    }

    public async ValueTask<bool> HasAnyOngoingCommandAsync(DiscordUser user, string pool)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(user, pool);
        var ongoingCommands = await redis.StringGetAsync(key);

        return ongoingCommands.HasValue && (long)ongoingCommands > 0;
    }

    public async ValueTask RemoveOngoingCommandAsync(DiscordUser user, string pool)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(user, pool);
        await redis.StringSetAsync(key, "0");
    }
}
