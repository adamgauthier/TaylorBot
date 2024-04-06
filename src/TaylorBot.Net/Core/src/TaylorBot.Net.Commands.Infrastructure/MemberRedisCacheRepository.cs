using StackExchange.Redis;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Infrastructure;

public class MemberRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, MemberPostgresRepository memberPostgresRepository) : IMemberTrackingRepository
{
    public async ValueTask<bool> AddOrUpdateMemberAsync(DiscordMember member, DateTimeOffset? lastSpokeAt)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = $"command-user:guild:{member.Member.GuildId}:user:{member.User.Id}";
        var cachedMember = await redis.StringGetAsync(key);

        if (!cachedMember.HasValue)
        {
            var memberAdded = await memberPostgresRepository.AddOrUpdateMemberAsync(member, lastSpokeAt);
            await redis.StringSetAsync(
                key,
                true,
                TimeSpan.FromHours(1)
            );
            return memberAdded;
        }

        return false;
    }
}
