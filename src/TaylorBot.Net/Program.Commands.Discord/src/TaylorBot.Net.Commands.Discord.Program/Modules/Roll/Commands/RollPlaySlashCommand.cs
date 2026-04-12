using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Random;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;

public class RollPlaySlashCommand(IRollStatsRepository rollStatsRepository, IRateLimiter rateLimiter, ICryptoSecureRandom cryptoSecureRandom) : ISlashCommand<NoOptions>
{
    public const string PrefixCommandName = "roll";

    public static string CommandName => "roll play";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public Command Play(RunContext context) => new(
        new(Info.Name, Aliases: [PrefixCommandName]),
        async () =>
        {
            var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(context.User, "roll");
            if (rateLimitResult != null)
                return rateLimitResult;

            var roll = cryptoSecureRandom.GetInt32(0, 1989);

            string color;
            int reward;
            string? flair = null;

            switch (roll)
            {
                case 0001:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    flair = "✨ *In my defense, I have none*";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 0007:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    flair = "🍂 *Please picture me in the trees...*";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 0013:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    flair = "🍀 *Lucky number 13*";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 0015:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    flair = "🎒 *You just might find who you're supposed to be*";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 0022:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    flair = "🥳 *I don't know about you, but I'm feeling 22*";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 0429:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    flair = "🗓️ *Do you really want to know where I was April 29th?*";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 0709:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    flair = "💋 *That July 9th, the beat of your heart...*";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 1213:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    flair = "🎂 *Taylor Day*";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 1989:
                    reward = AnniversaryEvent.IsActive ? 10_000 : 5_000;
                    color = "#00c3ff";
                    flair = "💎 *I think I am finally clean*";
                    await rollStatsRepository.WinPerfectRollAsync(context.User, reward);
                    break;

                default:
                    reward = 0;
                    color = "#f04747";
                    await rollStatsRepository.AddRollCountAsync(context.User);
                    break;
            }

            List<string> numberEmoji = ["0⃣", "1⃣", "2⃣", "3⃣", "4⃣", "5⃣", "6⃣", "7⃣", "8⃣", "9⃣"];
            var paddedRoll = $"{roll:D4}";

            return new EmbedResult(new EmbedBuilder()
                .WithColor(DiscordColor.FromHexString(color))
                .WithDescription(
                    $"""
                    ## 🎲 {string.Join("", paddedRoll.Select(digit => numberEmoji[byte.Parse($"{digit}")]))} 🎲
                    {(reward == 0
                        ? "Better luck next time! 😕"
                        : $"{flair}\nYou won {"taypoint".ToQuantity(reward, TaylorBotFormats.BoldReadable)}! 💰"
                    )}
                    """)
                .Build());
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(Play(context));
    }
}
