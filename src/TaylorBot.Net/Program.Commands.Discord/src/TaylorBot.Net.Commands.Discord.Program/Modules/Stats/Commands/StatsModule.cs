using Discord;
using Discord.Commands;
using System.Globalization;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Commands
{
    [Name("Stats 📊")]
    public class StatsModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly IServerStatsRepository _serverStatsRepository;
        private readonly IBotInfoRepository _botInfoRepository;

        public StatsModule(ICommandRunner commandRunner, IServerStatsRepository serverStatsRepository, IBotInfoRepository botInfoRepository)
        {
            _commandRunner = commandRunner;
            _serverStatsRepository = serverStatsRepository;
            _botInfoRepository = botInfoRepository;
        }

        [Command("serverstats")]
        [Alias("sstats", "genderstats", "agestats")]
        [Summary("Gets age and gender stats for a server.")]
        public async Task<RuntimeResult> ServerStatsAsync()
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var ageStats = await _serverStatsRepository.GetAgeStatsInGuildAsync(Context.Guild);
                    GenderStats genderStats = await _serverStatsRepository.GetGenderStatsInGuildAsync(Context.Guild);

                    string FormatPercent(long count) => ((decimal)count / genderStats.TotalCount).ToString("0.00%", CultureInfo.InvariantCulture);

                    return new EmbedResult(new EmbedBuilder()
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
                },
                Preconditions: new[] { new InGuildPrecondition() }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("botinfo")]
        [Alias("version", "invite")]
        [Summary("Gets general information about TaylorBot.")]
        public async Task<RuntimeResult> BotInfoAsync()
        {
            var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
            {
                var productVersion = await _botInfoRepository.GetProductVersionAsync();
                var applicationInfo = await Context.Client.GetApplicationInfoAsync();

                return new EmbedResult(new EmbedBuilder()
                    .WithUserAsAuthor(Context.Client.CurrentUser)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(applicationInfo.Description)
                    .AddField("Version", productVersion, inline: true)
                    .AddField("Author", applicationInfo.Owner.Mention, inline: true)
                    .AddField("Invite Link", "https://taylorbot.app/", inline: true)
                .Build());
            });

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
