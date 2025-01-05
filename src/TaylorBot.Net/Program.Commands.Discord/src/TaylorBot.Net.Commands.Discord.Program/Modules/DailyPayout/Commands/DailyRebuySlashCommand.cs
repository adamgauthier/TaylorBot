using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

public class DailyRebuySlashCommand(IDailyPayoutRepository dailyPayoutRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "daily rebuy";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var streakInfo = await dailyPayoutRepository.GetStreakInfoAsync(context.User);

                if (!streakInfo.HasValue)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        You've never claimed your daily reward! ❌
                        Use {context.MentionCommand("daily claim")} to start!
                        """));
                }
                else if (streakInfo.Value.MaxStreak > streakInfo.Value.CurrentStreak)
                {
                    const int RebuyPricePerDay = 50;
                    var cost = streakInfo.Value.MaxStreak * RebuyPricePerDay;

                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to buy back your daily streak of {streakInfo.Value.MaxStreak.ToString(TaylorBotFormats.BoldReadable)}?
                            This will cost you {"taypoint".ToQuantity(cost, TaylorBotFormats.BoldReadable)}.
                            """)),
                        confirm: async () =>
                        {
                            var result = await dailyPayoutRepository.RebuyMaxStreakAsync(context.User, RebuyPricePerDay);

                            if (result.IsSuccess)
                            {
                                return new MessageContent(EmbedFactory.CreateSuccess(
                                    $"""
                                    Successfully reset your daily streak back to {result.Value.CurrentDailyStreak.ToString(TaylorBotFormats.BoldReadable)}! 👍
                                    You now have {"taypoint".ToQuantity(result.Value.TotalTaypointCount, TaylorBotFormats.BoldReadable)}.
                                    """));
                            }
                            else
                            {
                                return new MessageContent(EmbedFactory.CreateError(
                                    $"""
                                    Could not rebuy your streak. You only have {"taypoint".ToQuantity(result.Error.TotalTaypointCount, TaylorBotFormats.BoldReadable)}. 😕
                                    You need {"taypoint".ToQuantity(cost, TaylorBotFormats.BoldReadable)}.
                                    """));
                            }
                        }
                    );
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        Your current daily streak is the highest it's ever been ({streakInfo.Value.CurrentStreak.ToString(TaylorBotFormats.BoldReadable)})! ⭐
                        There is nothing to buy back!
                        """));
                }
            }
        ));
    }
}
