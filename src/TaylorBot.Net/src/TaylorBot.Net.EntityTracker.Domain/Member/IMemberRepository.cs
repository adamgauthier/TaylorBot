using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.Member
{
    public interface IMemberRepository
    {
        Task AddNewMemberAsync(IGuildUser member);
        Task<MemberAddResult> AddNewMemberIfNotAddedAsync(IGuildUser member);
        Task SetMemberDeadAsync(IGuildUser member);
        Task UpdateDeadMembersAsync(IGuild guild, IEnumerable<IGuildUser> aliveMembers);
    }
}
