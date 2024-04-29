using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

public class DailyStreakSlashCommand(IDailyPayoutRepository dailyPayoutRepository) : ISlashCommand<DailyStreakSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("daily streak");

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = options.user.User;
                var streakInfo = await dailyPayoutRepository.GetStreakInfoAsync(user);

                if (!streakInfo.HasValue)
                {
                    return new EmbedResult(EmbedFactory.CreateSuccess(
                        $"""
                        {user.Mention} has never claimed their daily reward! ❌
                        Use {context.MentionCommand("daily claim")} to claim your daily reward!
                        """));
                }
                else if (streakInfo.Value.MaxStreak > streakInfo.Value.CurrentStreak)
                {
                    return new EmbedResult(EmbedFactory.CreateSuccess(
                        $"""
                        {user.Mention}'s current streak is {"day".ToQuantity(streakInfo.Value.CurrentStreak, TaylorBotFormats.BoldReadable)}! ⭐
                        Their highest streak ever is {"day".ToQuantity(streakInfo.Value.MaxStreak, TaylorBotFormats.BoldReadable)}! 🥇
                        """));
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateSuccess(
                        $"{user.Mention}'s current streak is the highest it's ever been ({"day".ToQuantity(streakInfo.Value.CurrentStreak, TaylorBotFormats.BoldReadable)})! ⭐"
                    ));
                }
            }
        ));
    }
}
