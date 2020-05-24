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

    public interface IWillAddResult { }

    public class WillAddedResult : IWillAddResult { }

    public class WillNotAddedResult : IWillAddResult
    {
        public SnowflakeId CurrentBeneficiaryId { get; }

        public WillNotAddedResult(SnowflakeId currentBeneficiaryId)
        {
            CurrentBeneficiaryId = currentBeneficiaryId;
        }
    }

    public interface IWillRemoveResult { }

    public class WillRemovedResult : IWillRemoveResult
    {
        public SnowflakeId RemovedBeneficiaryId { get; }

        public WillRemovedResult(SnowflakeId removedBeneficiaryId)
        {
            RemovedBeneficiaryId = removedBeneficiaryId;
        }
    }

    public class WillNotRemovedResult : IWillRemoveResult { }

    public interface ITaypointWillRepository
    {
        ValueTask<IWillAddResult> AddWillAsync(IUser owner, IUser beneficiary);
        ValueTask<IWillRemoveResult> RemoveWillWithOwnerAsync(IUser owner);
        ValueTask<IReadOnlyCollection<Will>> GetWillsWithBeneficiaryAsync(IUser beneficiary);
        ValueTask<IReadOnlyCollection<Transfer>> TransferAllPointsAsync(IReadOnlyCollection<SnowflakeId> fromUserIds, IUser toUser);
        ValueTask RemoveWillsWithBeneficiaryAsync(IReadOnlyCollection<SnowflakeId> ownerUserIds, IUser beneficiary);
    }
}
