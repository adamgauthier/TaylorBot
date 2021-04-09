using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.ServerStats.Domain
{
    public record AgeStats(decimal? AgeAverage, decimal? AgeMedian);

    public record GenderStats(long TotalCount, long MaleCount, long FemaleCount, long OtherCount);

    public interface IServerStatsRepository
    {
        ValueTask<AgeStats> GetAgeStatsInGuildAsync(IGuild guild);
        ValueTask<GenderStats> GetGenderStatsInGuildAsync(IGuild guild);
    }
}
