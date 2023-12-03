using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;

public record TransferResult(long OriginalCount, long GiftedCount, long ReceiverNewCount);

public interface ITaypointTransferRepository
{
    ValueTask<TransferResult> TransferTaypointsAsync(IUser from, IUser to, ITaypointAmount amount);
}

