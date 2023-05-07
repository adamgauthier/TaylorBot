using Discord;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure;

public class MemberRedisCacheRepository : IMemberTrackingRepository
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private readonly MemberPostgresRepository _memberPostgresRepository;

    public MemberRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, MemberPostgresRepository memberPostgresRepository)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _memberPostgresRepository = memberPostgresRepository;
    }

    public async ValueTask<bool> AddOrUpdateMemberAsync(IGuildUser member, DateTimeOffset? lastSpokeAt)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = $"command-user:guild:{member.GuildId}:user:{member.Id}";
        var cachedMember = await redis.StringGetAsync(key);

        if (!cachedMember.HasValue)
        {
            var memberAdded = await _memberPostgresRepository.AddOrUpdateMemberAsync(member, lastSpokeAt);
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
