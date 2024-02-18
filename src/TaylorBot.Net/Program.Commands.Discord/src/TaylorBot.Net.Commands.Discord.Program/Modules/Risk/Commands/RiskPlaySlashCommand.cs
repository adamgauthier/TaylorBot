using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Random;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;

public record RiskResult(long invested_count, long final_count, long profit_count);

public record RiskProfile(long gamble_win_count, long gamble_win_amount, long gamble_lose_count, long gamble_lose_amount);

public record RiskLeaderboardEntry(string user_id, string username, long gamble_win_count, long rank);

public interface IRiskStatsRepository
{
    Task<RiskProfile?> GetProfileAsync(IUser user);
    Task<RiskResult> WinAsync(IUser user, ITaypointAmount amount, RiskLevel level);
    Task<RiskResult> LoseAsync(IUser user, ITaypointAmount amount);
    Task<IList<RiskLeaderboardEntry>> GetLeaderboardAsync(IGuild guild);
}

public class RiskPlaySlashCommand(TaypointAmountParser amountParser, IRiskStatsRepository riskStatsRepository, ICryptoSecureRandom cryptoSecureRandom, IPseudoRandom pseudoRandom) : ISlashCommand<RiskPlaySlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("risk play");

    public record Options(ITaypointAmount amount, RiskLevel? level);

    public Command Play(RunContext context, IUser author, RiskLevel? level, ITaypointAmount? amount, string? amountString = null) => new(
        new(Info.Name),
        async () =>
        {
            if (amountString != null)
            {
                var parsed = await amountParser.ParseStringAsync(context, amountString);
                if (!parsed)
                {
                    return new EmbedResult(EmbedFactory.CreateError($"`amount`: {parsed.Error.Message}"));
                }
                amount = parsed.Value;
            }
            ArgumentNullException.ThrowIfNull(amount);

            level ??= RiskLevel.Low;

            int winThreshold = level switch
            {
                RiskLevel.Low => 51,
                RiskLevel.Moderate => 76,
                RiskLevel.High => 91,
                _ => throw new NotImplementedException(),
            };

            var randomNumber = cryptoSecureRandom.GetInt32(1, 100);

            var won = randomNumber >= winThreshold;

            var result = won
                ? await riskStatsRepository.WinAsync(author, amount, level.Value)
                : await riskStatsRepository.LoseAsync(author, amount);

            var originalCount = result.final_count - result.profit_count;

            var reason = pseudoRandom.GetRandomElement(won ? WinReasons : LoseReasons);

            var embed = new EmbedBuilder()
                .WithColor(won ? TaylorBotColors.SuccessColor : TaylorBotColors.ErrorColor)
                .WithDescription(
                    $"""
                    ### Opportunity ({level} Risk)
                    {reason.Opportunity}
                    You invest: **{"taypoint".ToQuantity(result.invested_count, TaylorBotFormats.Readable)} ({GetPercent(originalCount, result.invested_count):0%} of balance)** 💵
                    ### Outcome
                    **{(result.profit_count >= 0 ? "🟢 +" : "🔴 —")}{"taypoint".ToQuantity(Math.Abs(result.profit_count), TaylorBotFormats.Readable)}**
                    {reason.Outcome} {(won ? "💰" : "💸")}
                    Your balance: {originalCount.ToString(TaylorBotFormats.BoldReadable)} ➡️ {"taypoint".ToQuantity(result.final_count, TaylorBotFormats.BoldReadable)} {(won ? "📈" : "📉")}
                    """);

            return new EmbedResult(embed.Build());
        }
    );

    private static double GetPercent(long originalCount, long investedCount)
    {
        return originalCount != 0 ? (double)investedCount / originalCount : 0;
    }

    private record Reason(string Opportunity, string Outcome);

    private static readonly Reason[] WinReasons = [
        new(
            "A member from your favorite finance Discord server tells you about a secret underground stock exchange with exclusive investment opportunities 🔮",
            "To your surprise, the stocks soar!"),
        new(
            "A mysterious woman in an alley offers you a chance to invest in an unknown startup with promises of cutting-edge AI technology 🤖",
            "The startup becomes a tech giant!"),
        new(
            "While browsing a community board, you find a post discussing a new decentralized cryptocurrency 🪙",
            "The cryptocurrency gains widespread adoption!"),
        new(
            "A charismatic guy in a bar shares information about an upcoming virtual reality gaming platform 🥽",
            "The platform becomes a massive success!"),
        new(
            "An anonymous message in a chat room hints at a hidden opportunity to invest in a futuristic energy project ⚡",
            "The project is a groundbreaking success!"),
    ];

    private static readonly Reason[] LoseReasons = [
        new(
            "As you stroll through an online marketplace, an anonymous user sends you a message claiming to have insider information on a groundbreaking stock 🤔",
            "It was a pump-and-dump scheme, leaving you with worthless shares"),
        new(
            "At a finance convention, a charismatic man invites you to invest in a new cryptocurrency promising unprecedented returns 🪙",
            "You wake up the next day and find the cryptocurrency has collapsed"),
        new(
            "A mysterious email lands in your inbox, offering a chance to invest in a startup developing revolutionary health supplements 💊",
            "The company was a front for an elaborate pyramid scheme"),
        new(
            "During a business conference, an enthusiastic entrepreneur pitches a real estate property in a rapidly developing city 🏚️",
            "The property suddenly collapses, and your investment vanishes"),
        new(
            "While exploring a forum, you come across a post about a rare artifact with the potential to skyrocket in value 🏺",
            "The market quickly saturates and the artifact's value plummets"),
    ];

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Play(context, context.User, options.level, options.amount));
    }
}
