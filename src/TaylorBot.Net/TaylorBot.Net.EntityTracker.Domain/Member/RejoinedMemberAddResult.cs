using System;

namespace TaylorBot.Net.EntityTracker.Domain.Member
{
    public class RejoinedMemberAddResult : MemberAddResult
    {
        public DateTimeOffset FirstJoinedAt { get; }

        public RejoinedMemberAddResult(DateTimeOffset firstJoinedAt)
        {
            FirstJoinedAt = firstJoinedAt;
        }
    }
}
