using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain
{
    public class Will
    {
        public SnowflakeId OwnerUserId { get; }
        public DateTimeOffset OwnerLatestSpokeAt { get; }

        public Will(SnowflakeId ownerUserId, DateTimeOffset ownerLatestSpokeAt)
        {
            OwnerUserId = ownerUserId;
            OwnerLatestSpokeAt = ownerLatestSpokeAt;
        }
    }

    public class Transfer
    {
        public SnowflakeId UserId { get; }
        public long TaypointCount { get; }
        public long OriginalTaypointCount { get; }

        public Transfer(SnowflakeId userId, long taypointCount, long originalTaypointCount)
        {
            UserId = userId;
            TaypointCount = taypointCount;
            OriginalTaypointCount = originalTaypointCount;
        }
    }

    public interface ITaypointWillRepository
    {
        ValueTask<IReadOnlyCollection<Will>> GetWillsWithBeneficiaryAsync(IUser beneficiary);
        ValueTask<IReadOnlyCollection<Transfer>> TransferAllPointsAsync(IReadOnlyCollection<SnowflakeId> fromUserIds, IUser toUser);
        ValueTask RemoveWillsAsync(IReadOnlyCollection<SnowflakeId> ownerUserIds, IUser beneficiary);
    }
}
