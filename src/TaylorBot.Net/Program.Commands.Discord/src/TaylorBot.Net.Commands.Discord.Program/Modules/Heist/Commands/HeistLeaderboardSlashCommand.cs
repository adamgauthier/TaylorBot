using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;
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

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class HeistLeaderboardSlashCommand(IHeistStatsRepository heistStatsRepository, MemberNotInGuildUpdater memberNotInGuildUpdater) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("heist leaderboard");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var leaderboard = await heistStatsRepository.GetLeaderboardAsync(guild);

                memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                    nameof(HeistLeaderboardSlashCommand),
                    guild,
                    leaderboard.Select(e => new SnowflakeId(e.user_id)).ToList());

                var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.rank}. {entry.username.MdUserLink(entry.user_id)}: {"win".ToQuantity(entry.heist_win_count, TaylorBotFormats.BoldReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithGuildAsAuthor(guild)
                    .WithTitle("Heist Wins Leaderboard 💼");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                        $"""
                        No heists played by members of this server.
                        Members need to use {context.MentionCommand("heist play")}! 😊
                        """)),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: [
                new InGuildPrecondition(),
            ]
        ));
    }
}
