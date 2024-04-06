using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;

public record HeistResult(string UserId, long InvestedCount, long FinalCount, long ProfitCount);

public record HeistProfile(long heist_win_count, long heist_win_amount, long heist_lose_count, long heist_lose_amount);

public record HeistLeaderboardEntry(string user_id, string username, long heist_win_count, long rank);

public interface IHeistStatsRepository
{
    Task<HeistProfile?> GetProfileAsync(IUser user);
    Task<List<HeistResult>> WinHeistAsync(IList<HeistPlayer> players, string payoutMultiplier);
    Task<List<HeistResult>> LoseHeistAsync(IList<HeistPlayer> players);
    Task<IList<HeistLeaderboardEntry>> GetLeaderboardAsync(CommandGuild guild);
}
