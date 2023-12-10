namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;

public record HeistResult(string UserId, long InvestedCount, long FinalCount, long ProfitCount);

public interface IHeistStatsRepository
{
    Task<List<HeistResult>> WinHeistAsync(IList<HeistPlayer> players, string payoutMultiplier);
    Task<List<HeistResult>> LoseHeistAsync(IList<HeistPlayer> players);
}

