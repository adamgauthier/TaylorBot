using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain
{
    public class WillOwner
    {
        public SnowflakeId OwnerUserId { get; }
        public string OwnerUsername { get; }
        public DateTimeOffset OwnerLatestSpokeAt { get; }

        public WillOwner(SnowflakeId ownerUserId, string ownerUsername, DateTimeOffset ownerLatestSpokeAt)
        {
            OwnerUserId = ownerUserId;
            OwnerUsername = ownerUsername;
            OwnerLatestSpokeAt = ownerLatestSpokeAt;
        }
    }

    public class Transfer
    {
        public SnowflakeId UserId { get; }
        public string Username { get; }
        public long TaypointCount { get; }
        public long OriginalTaypointCount { get; }

        public Transfer(SnowflakeId userId, string username, long taypointCount, long originalTaypointCount)
        {
            UserId = userId;
            Username = username;
            TaypointCount = taypointCount;
            OriginalTaypointCount = originalTaypointCount;
        }
    }

    public interface IWillAddResult { }

    public class WillAddedResult : IWillAddResult { }

    public class WillNotAddedResult : IWillAddResult
    {
        public SnowflakeId CurrentBeneficiaryId { get; }
        public string CurrentBeneficiaryUsername { get; }

        public WillNotAddedResult(SnowflakeId currentBeneficiaryId, string currentBeneficiaryUsername)
        {
            CurrentBeneficiaryId = currentBeneficiaryId;
            CurrentBeneficiaryUsername = currentBeneficiaryUsername;
        }
    }

    public interface IWillRemoveResult { }

    public class WillRemovedResult : IWillRemoveResult
    {
        public SnowflakeId RemovedBeneficiaryId { get; }
        public string RemovedBeneficiaryUsername { get; }

        public WillRemovedResult(SnowflakeId removedBeneficiaryId, string removedBeneficiaryUsername)
        {
            RemovedBeneficiaryId = removedBeneficiaryId;
            RemovedBeneficiaryUsername = removedBeneficiaryUsername;
        }
    }

    public class WillNotRemovedResult : IWillRemoveResult { }

    public class Will
    {
        public SnowflakeId BeneficiaryUserId { get; }
        public string BeneficiaryUsername { get; }

        public Will(SnowflakeId beneficiaryUserId, string beneficiaryUsername)
        {
            BeneficiaryUserId = beneficiaryUserId;
            BeneficiaryUsername = beneficiaryUsername;
        }
    }

    public interface ITaypointWillRepository
    {
        ValueTask<Will?> GetWillAsync(IUser owner);
        ValueTask<IWillAddResult> AddWillAsync(IUser owner, IUser beneficiary);
        ValueTask<IWillRemoveResult> RemoveWillWithOwnerAsync(IUser owner);
        ValueTask<IReadOnlyCollection<WillOwner>> GetWillsWithBeneficiaryAsync(IUser beneficiary);
        ValueTask<IReadOnlyCollection<Transfer>> TransferAllPointsAsync(IReadOnlyCollection<SnowflakeId> fromUserIds, IUser toUser);
        ValueTask RemoveWillsWithBeneficiaryAsync(IReadOnlyCollection<SnowflakeId> ownerUserIds, IUser beneficiary);
    }
}
