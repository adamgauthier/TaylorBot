using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.EntityTracker.Domain.Member;

public interface IMemberRepository
{
    ValueTask<bool> AddNewMemberAsync(IGuildUser member);
    ValueTask<MemberAddResult> AddNewMemberOrUpdateAsync(IGuildUser member);
    ValueTask UpdateMembersNotInGuildAsync(IGuild guild, IList<SnowflakeId> members);
}
