using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain
{
    public record WillOwner(SnowflakeId OwnerUserId, string OwnerUsername, DateTimeOffset OwnerLatestSpokeAt);

    public record Transfer(SnowflakeId UserId, string Username, long TaypointCount, long OriginalTaypointCount);

    public interface IWillAddResult { }

    public record WillAddedResult : IWillAddResult;

    public record WillNotAddedResult(SnowflakeId CurrentBeneficiaryId, string CurrentBeneficiaryUsername) : IWillAddResult;

    public interface IWillRemoveResult { }

    public record WillRemovedResult(SnowflakeId RemovedBeneficiaryId, string RemovedBeneficiaryUsername) : IWillRemoveResult;

    public record WillNotRemovedResult : IWillRemoveResult;

    public record Will(SnowflakeId BeneficiaryUserId, string BeneficiaryUsername);

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
