using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;

public record WillOwner(SnowflakeId OwnerUserId, string OwnerUsername, DateTimeOffset OwnerLatestSpokeAt);

public record Transfer(SnowflakeId UserId, string Username, long TaypointCount, long OriginalTaypointCount);

public interface IWillRemoveResult { }

public record WillRemovedResult(SnowflakeId RemovedBeneficiaryId, string RemovedBeneficiaryUsername) : IWillRemoveResult;

public record WillNotRemovedResult : IWillRemoveResult;

public record Will(SnowflakeId BeneficiaryUserId, string BeneficiaryUsername);

public interface ITaypointWillRepository
{
    ValueTask<Will?> GetWillAsync(DiscordUser owner);
    ValueTask AddWillAsync(DiscordUser owner, DiscordUser beneficiary);
    ValueTask<IWillRemoveResult> RemoveWillWithOwnerAsync(DiscordUser owner);
    ValueTask<IReadOnlyCollection<WillOwner>> GetWillsWithBeneficiaryAsync(DiscordUser beneficiary);
    ValueTask<IReadOnlyCollection<Transfer>> TransferAllPointsAsync(IReadOnlyCollection<SnowflakeId> fromUserIds, DiscordUser toUser);
    ValueTask RemoveWillsWithBeneficiaryAsync(IReadOnlyCollection<SnowflakeId> ownerUserIds, DiscordUser beneficiary);
}
