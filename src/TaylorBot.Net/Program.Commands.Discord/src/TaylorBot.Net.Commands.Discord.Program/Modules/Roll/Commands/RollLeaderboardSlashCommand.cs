using Discord;
using Humanizer;
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

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;

public class RollLeaderboardSlashCommand(IRollStatsRepository rollStatsRepository, MemberNotInGuildUpdater memberNotInGuildUpdater) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("roll leaderboard");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var leaderboard = await rollStatsRepository.GetLeaderboardAsync(guild);

                memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                    nameof(RollLeaderboardSlashCommand),
                    guild,
                    leaderboard.Select(e => new SnowflakeId(e.user_id)).ToList());

                var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.rank}\\. {entry.username.MdUserLink(entry.user_id)}: {"perfect roll".ToQuantity(entry.perfect_roll_count, TaylorBotFormats.BoldReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithGuildAsAuthor(guild)
                    .WithTitle("Roll Leaderboard 🍀");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                        $"""
                        No roll played by members of this server.
                        Members need to use {context.MentionCommand("roll play")}! 😊
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
