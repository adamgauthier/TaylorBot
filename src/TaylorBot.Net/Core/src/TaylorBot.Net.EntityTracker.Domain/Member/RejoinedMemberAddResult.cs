namespace TaylorBot.Net.EntityTracker.Domain.Member;

public class RejoinedMemberAddResult(DateTimeOffset firstJoinedAt) : MemberAddResult
{
    public DateTimeOffset FirstJoinedAt { get; } = firstJoinedAt;
}
