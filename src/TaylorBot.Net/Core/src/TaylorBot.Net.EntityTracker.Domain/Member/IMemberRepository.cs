using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.Member
{
    public interface IMemberRepository
    {
        ValueTask<bool> AddNewMemberAsync(IGuildUser member);
        ValueTask<MemberAddResult> AddNewMemberOrUpdateAsync(IGuildUser member);
        ValueTask SetMemberDeadAsync(IGuild guild, IUser user);
    }
}
