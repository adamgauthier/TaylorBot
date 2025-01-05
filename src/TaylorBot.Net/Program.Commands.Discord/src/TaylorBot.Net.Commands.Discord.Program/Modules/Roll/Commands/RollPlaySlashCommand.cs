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

            switch (roll)
            {
                case 1:
                case 7:
                case 13:
                case 15:
                case 22:
                case 420:
                    reward = AnniversaryEvent.IsActive ? 200 : 100;
                    color = "#43b581";
                    await rollStatsRepository.WinRollAsync(context.User, reward);
                    break;

                case 1989:
                    reward = AnniversaryEvent.IsActive ? 10_000 : 5_000;
                    color = "#00c3ff";
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
                .WithTitle("Rolling the Taylor Machine 🎲")
                .WithDescription(
                    $"""
                    You get: {string.Join("", paddedRoll.Select(digit => numberEmoji[byte.Parse($"{digit}")]))}
                    {(reward == 0
                        ? "Better luck next time! 😕"
                        : $"You won {"taypoint".ToQuantity(reward, TaylorBotFormats.BoldReadable)}! 💰"
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
