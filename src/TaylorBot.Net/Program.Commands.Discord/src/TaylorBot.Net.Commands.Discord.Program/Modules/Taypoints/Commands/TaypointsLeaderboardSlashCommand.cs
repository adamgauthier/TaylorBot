using Discord;
using Humanizer;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class TaypointsLeaderboardSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("taypoints leaderboard");

    private readonly ITaypointBalanceRepository _taypointBalanceRepository;

    public TaypointsLeaderboardSlashCommand(ITaypointBalanceRepository taypointBalanceRepository)
    {
        _taypointBalanceRepository = taypointBalanceRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var leaderboard = await _taypointBalanceRepository.GetLeaderboardAsync(context.Guild!);

                var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.Rank}: {entry.Username.MdUserLink(entry.UserId)} - {"taypoint".ToQuantity(entry.TaypointCount, TaylorBotFormats.CodedReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
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
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
            }
        ));
    }
}
