using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.Member
{
    public interface IMemberRepository
    {
        Task<bool> AddNewMemberAsync(IGuildUser member);
        Task<MemberAddResult> AddNewMemberOrUpdateAsync(IGuildUser member);
        Task SetMemberDeadAsync(IGuildUser member);
    }
}
