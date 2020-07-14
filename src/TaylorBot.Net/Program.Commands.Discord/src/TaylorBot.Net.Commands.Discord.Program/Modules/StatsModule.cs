using Discord;
using Discord.Commands;
using System.Globalization;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.ServerStats.Domain;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Stats 📊")]
    public class StatsModule : TaylorBotModule
    {
        private readonly IServerStatsRepository _serverStatsRepository;

        public StatsModule(IServerStatsRepository serverStatsRepository)
        {
            _serverStatsRepository = serverStatsRepository;
        }

        [RequireInGuild]
        [Command("serverstats")]
        [Alias("sstats", "genderstats", "agestats")]
        [Summary("Gets age and gender stats for a server.")]
        public async Task<RuntimeResult> ServerStatsAsync()
        {
            var ageStats = await _serverStatsRepository.GetAgeStatsInGuildAsync(Context.Guild);
            GenderStats genderStats = await _serverStatsRepository.GetGenderStatsInGuildAsync(Context.Guild);

            string FormatPercent(long count) => ((decimal)count / genderStats.TotalCount).ToString("0.00%", CultureInfo.InvariantCulture);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithGuildAsAuthor(Context.Guild)
                .WithColor(TaylorBotColors.SuccessColor)
                .AddField("Age", string.Join('\n', new[] {
                    $"Median: {(ageStats.AgeMedian.HasValue ? ageStats.AgeMedian.Value.ToString() : "No Data")}",
                    $"Average: {(ageStats.AgeAverage.HasValue ? ageStats.AgeAverage.Value.ToString() : "No Data")}"
                }), inline: true)
                .AddField("Gender", string.Join('\n', new[] {
                    $"Male: {genderStats.MaleCount}{(genderStats.TotalCount != 0 ? $" ({FormatPercent(genderStats.MaleCount)})" : string.Empty)}",
                    $"Female: {genderStats.FemaleCount}{(genderStats.TotalCount != 0 ? $" ({FormatPercent(genderStats.FemaleCount)})" : string.Empty)}",
                    $"Other: {genderStats.OtherCount}{(genderStats.TotalCount != 0 ? $" ({FormatPercent(genderStats.OtherCount)})" : string.Empty)}"
                }), inline: true)
            .Build());
        }
    }
}
