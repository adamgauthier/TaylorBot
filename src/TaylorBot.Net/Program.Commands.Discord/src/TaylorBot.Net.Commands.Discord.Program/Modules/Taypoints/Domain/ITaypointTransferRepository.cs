using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;

public record TransferResult(long OriginalCount, long GiftedCount, long ReceiverNewCount);

public interface ITaypointTransferRepository
{
    ValueTask<TransferResult> TransferTaypointsAsync(IUser from, IUser to, int taypointCount);
}

