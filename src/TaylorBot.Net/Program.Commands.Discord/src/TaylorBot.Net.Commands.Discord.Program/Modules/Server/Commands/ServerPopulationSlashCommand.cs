using Discord;
using System.Globalization;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public class ServerPopulationSlashCommand(IServerStatsRepository serverStatsRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "server population";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);
                var guild = context.Guild;

                var ageStats = await serverStatsRepository.GetAgeStatsInGuildAsync(guild);
                var genderStats = await serverStatsRepository.GetGenderStatsInGuildAsync(guild);

                string FormatPercent(long count) => ((decimal)count / genderStats.TotalCount).ToString("0.0%", CultureInfo.InvariantCulture);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .AddField(
                        "Age",
                        $"""
                        Median: {(ageStats.AgeMedian.HasValue ? $"{ageStats.AgeMedian.Value}" : "No Data")}
                        Average: {(ageStats.AgeAverage.HasValue ? $"{ageStats.AgeAverage.Value}" : "No Data")}
                        """,
                        inline: true)
                    .AddField(
                        "Gender",
                        $"""
                        Male: {genderStats.MaleCount}{(genderStats.TotalCount != 0 ? $" ({FormatPercent(genderStats.MaleCount)})" : string.Empty)}
                        Female: {genderStats.FemaleCount}{(genderStats.TotalCount != 0 ? $" ({FormatPercent(genderStats.FemaleCount)})" : string.Empty)}
                        Other: {genderStats.OtherCount}{(genderStats.TotalCount != 0 ? $" ({FormatPercent(genderStats.OtherCount)})" : string.Empty)}
                        """,
                        inline: true);

                if (guild.Fetched != null)
                {
                    embed.WithGuildAsAuthor(guild.Fetched);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [
                new InGuildPrecondition(),
            ]
        ));
    }
}
