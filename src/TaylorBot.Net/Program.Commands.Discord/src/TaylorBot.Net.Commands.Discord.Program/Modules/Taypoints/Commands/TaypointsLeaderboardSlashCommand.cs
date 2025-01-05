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

public class TaypointsLeaderboardSlashCommand(
    ITaypointBalanceRepository taypointBalanceRepository,
    MemberNotInGuildUpdater memberNotInGuildUpdater,
    TaypointGuildCacheUpdater taypointGuildCacheUpdater,
    TaskExceptionLogger taskExceptionLogger) : ISlashCommand<NoOptions>
{
    public static string CommandName => "taypoints leaderboard";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild;
                ArgumentNullException.ThrowIfNull(guild?.Fetched);

                var leaderboard = await taypointBalanceRepository.GetLeaderboardAsync(guild);
                UpdateLastKnownPointCountsInBackground(guild, leaderboard);

                // We requested more rows than necessary to update their last known point counts
                var leaderboardToDisplay = leaderboard.Take(150).ToList();

                memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                    nameof(TaypointsLeaderboardSlashCommand),
                    guild.Fetched,
                    leaderboardToDisplay.Select(e => new SnowflakeId(e.user_id)).ToList());

                var pages = leaderboardToDisplay.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.rank}\\. {entry.username.MdUserLink(entry.user_id)}: {"taypoint".ToQuantity(entry.last_known_taypoint_count, TaylorBotFormats.BoldReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Taypoint Leaderboard 🪙");

                if (guild.Fetched != null)
                {
                    baseEmbed.WithGuildAsAuthor(guild.Fetched);
                }

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
                new InGuildPrecondition(botMustBeInGuild: true),
            ]
        ));
    }

    private void UpdateLastKnownPointCountsInBackground(CommandGuild guild, IList<TaypointLeaderboardEntry> leaderboard)
    {
        _ = taskExceptionLogger.LogOnError(
            async () =>
            {
                var updates = leaderboard
                    .Where(e => e.last_known_taypoint_count != e.taypoint_count)
                    .Select(e => new TaypointCountUpdate(e.user_id, e.taypoint_count))
                    .ToList();

                await taypointGuildCacheUpdater.UpdateLastKnownPointCountsAsync(guild, updates);

                await taypointGuildCacheUpdater.UpdateLastKnownPointCountsForRecentlyActiveMembersAsync(guild);
            },
            nameof(UpdateLastKnownPointCountsInBackground)
        );
    }
}
