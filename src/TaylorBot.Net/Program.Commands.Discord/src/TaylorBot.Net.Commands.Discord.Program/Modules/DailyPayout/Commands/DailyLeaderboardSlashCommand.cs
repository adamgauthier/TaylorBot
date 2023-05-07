using Discord;
using Humanizer;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

public class DailyLeaderboardSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("daily leaderboard");

    private readonly IDailyPayoutRepository _dailyPayoutRepository;
    private readonly MemberNotInGuildUpdater _memberNotInGuildUpdater;

    public DailyLeaderboardSlashCommand(IDailyPayoutRepository dailyPayoutRepository, MemberNotInGuildUpdater memberNotInGuildUpdater)
    {
        _dailyPayoutRepository = dailyPayoutRepository;
        _memberNotInGuildUpdater = memberNotInGuildUpdater;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var leaderboard = await _dailyPayoutRepository.GetLeaderboardAsync(guild);

                _memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                    nameof(DailyLeaderboardSlashCommand),
                    guild,
                    leaderboard.Select(e => e.UserId).ToList());

                var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.Rank}: {entry.Username.MdUserLink(entry.UserId)} - {"day".ToQuantity(entry.CurrentDailyStreak, TaylorBotFormats.CodedReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithGuildAsAuthor(guild)
                    .WithTitle("Daily Streak Leaderboard 📅");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                        $"""
                        No daily streaks in this server.
                        Members need to use {context.MentionCommand("daily claim")}! 😊
                        """)),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
            }
        ));
    }
}
