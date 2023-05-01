using Humanizer;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

public class DailyRebuySlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("daily rebuy");

    private readonly IDailyPayoutRepository _dailyPayoutRepository;

    public DailyRebuySlashCommand(IDailyPayoutRepository dailyPayoutRepository)
    {
        _dailyPayoutRepository = dailyPayoutRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var streakInfo = await _dailyPayoutRepository.GetStreakInfoAsync(context.User);

                if (!streakInfo.HasValue)
                {
                    return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                        "You've never claimed your daily reward! ❌",
                        $"Use {context.MentionCommand("daily claim")} to start!"
                    })));
                }
                else if (streakInfo.Value.MaxStreak > streakInfo.Value.CurrentStreak)
                {
                    const int RebuyPricePerDay = 50;
                    var cost = streakInfo.Value.MaxStreak * RebuyPricePerDay;

                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(string.Join('\n', new[] {
                            $"Are you sure you want to buy back your daily streak of **{streakInfo.Value.MaxStreak}**?",
                            $"This will cost you {"taypoint".ToQuantity(cost, TaylorBotFormats.BoldReadable)}."
                        }))),
                        confirm: async () =>
                        {
                            var result = await _dailyPayoutRepository.RebuyMaxStreakAsync(context.User, RebuyPricePerDay);

                            if (result.IsSuccess)
                            {
                                return new MessageContent(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                                    $"Successfully reset your daily streak back to **{result.Value.CurrentDailyStreak}**. Don't forget to claim it tomorrow! 👍",
                                    $"You now have {"taypoint".ToQuantity(result.Value.TotalTaypointCount, TaylorBotFormats.BoldReadable)}."
                                })));
                            }
                            else
                            {
                                return new MessageContent(EmbedFactory.CreateError(string.Join('\n', new[] {
                                    $"Could not rebuy your streak. You only have {"taypoint".ToQuantity(result.Error.TotalTaypointCount, TaylorBotFormats.BoldReadable)}. 😕",
                                    $"You need {"taypoint".ToQuantity(cost, TaylorBotFormats.BoldReadable)}."
                                })));
                            }
                        }
                    );
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                        $"Your current daily streak is the highest it's ever been (**{streakInfo.Value.CurrentStreak}**)! ⭐",
                        "There is nothing to buy back!"
                    })));
                }
            }
        ));
    }
}
