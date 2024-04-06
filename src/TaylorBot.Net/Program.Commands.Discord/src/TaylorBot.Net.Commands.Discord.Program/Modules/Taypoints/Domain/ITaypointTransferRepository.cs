using TaylorBot.Net.Core.User;
using static TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain.TransferResult;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;

public record TransferResult(long OriginalCount, long GiftedCount, IReadOnlyList<Recipient> Recipients)
{
    public record Recipient(string UserId, long Received, long UpdatedBalance);
}

public interface ITaypointTransferRepository
{
    ValueTask<TransferResult> TransferTaypointsAsync(DiscordUser from, IReadOnlyList<DiscordUser> to, ITaypointAmount amount);
}

