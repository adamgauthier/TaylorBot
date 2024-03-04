using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class TaypointsLeaderboardSlashCommand(ITaypointBalanceRepository taypointBalanceRepository, MemberNotInGuildUpdater memberNotInGuildUpdater, TaskExceptionLogger taskExceptionLogger) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("taypoints leaderboard");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var leaderboard = await taypointBalanceRepository.GetLeaderboardAsync(guild);

                memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                    nameof(TaypointsLeaderboardSlashCommand),
                    guild,
                    leaderboard.Select(e => new SnowflakeId(e.user_id)).ToList());

                UpdateLastKnownPointCountsInBackground(guild);

                var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.rank}\\. {entry.username.MdUserLink(entry.user_id)}: {"taypoint".ToQuantity(entry.last_known_taypoint_count, TaylorBotFormats.BoldReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithGuildAsAuthor(guild)
                    .WithTitle("Taypoint Leaderboard 🪙");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                        """
                        No member has taypoints in this server.
                        Members should automatically get points over time! 😊
                        """)),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: [
                new InGuildPrecondition(),
            ]
        ));
    }

    private void UpdateLastKnownPointCountsInBackground(IGuild guild)
    {
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            async () => await taypointBalanceRepository.UpdateLastKnownPointCountsAsync(guild),
            nameof(taypointBalanceRepository.UpdateLastKnownPointCountsAsync)
        ));
    }
}
