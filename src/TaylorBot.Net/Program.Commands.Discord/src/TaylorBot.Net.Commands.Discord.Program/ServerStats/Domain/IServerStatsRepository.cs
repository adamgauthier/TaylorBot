using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.ServerStats.Domain
{
    public class AgeStats
    {
        public decimal? AgeAverage { get; }
        public decimal? AgeMedian { get; }

        public AgeStats(decimal? ageAverage, decimal? ageMedian)
        {
            AgeAverage = ageAverage;
            AgeMedian = ageMedian;
        }
    }

    public class GenderStats
    {
        public long TotalCount { get; }
        public long MaleCount { get; }
        public long FemaleCount { get; }
        public long OtherCount { get; }

        public GenderStats(long totalCount, long maleCount, long femaleCount, long otherCount)
        {
            TotalCount = totalCount;
            MaleCount = maleCount;
            FemaleCount = femaleCount;
            OtherCount = otherCount;
        }
    }

    public interface IServerStatsRepository
    {
        ValueTask<AgeStats> GetAgeStatsInGuildAsync(IGuild guild);
        ValueTask<GenderStats> GetGenderStatsInGuildAsync(IGuild guild);
    }
}
