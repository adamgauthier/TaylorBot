namespace TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;

public record AgeStats(decimal? AgeAverage, decimal? AgeMedian);

public record GenderStats(long TotalCount, long MaleCount, long FemaleCount, long OtherCount);

public interface IServerStatsRepository
{
    ValueTask<AgeStats> GetAgeStatsInGuildAsync(CommandGuild guild);
    ValueTask<GenderStats> GetGenderStatsInGuildAsync(CommandGuild guild);
}
