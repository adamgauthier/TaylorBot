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
    Task<RiskResult> WinAsync(IUser user, ITaypointAmount amount);
    Task<RiskResult> LoseAsync(IUser user, ITaypointAmount amount);
    Task<IList<RiskLeaderboardEntry>> GetLeaderboardAsync(IGuild guild);
}

public class RiskPlaySlashCommand(TaypointAmountParser amountParser, IRiskStatsRepository riskStatsRepository, ICryptoSecureRandom cryptoSecureRandom, IPseudoRandom pseudoRandom) : ISlashCommand<RiskPlaySlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("risk play");

    public record Options(ITaypointAmount amount);

    public Command Play(RunContext context, IUser author, ITaypointAmount? amount, string? amountString = null) => new(
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

            var won = cryptoSecureRandom.GetInt32(0, 1) == 1;

            var result = won
                ? await riskStatsRepository.WinAsync(author, amount)
                : await riskStatsRepository.LoseAsync(author, amount);

            var originalCount = won
                ? result.final_count - result.invested_count
                : result.final_count + result.invested_count;

            var reason = pseudoRandom.GetRandomElement(won ? WinReasons : LoseReasons);

            var balance = $"{"taypoint".ToQuantity(result.invested_count, TaylorBotFormats.BoldReadable)} (**{GetPercent(originalCount, result.invested_count):0%}** of your balance)";

            var embed = new EmbedBuilder()
                .WithColor(won ? TaylorBotColors.SuccessColor : TaylorBotColors.ErrorColor)
                .WithDescription(
                    $"""
                    ### Investment Opportunity
                    {reason.Opportunity.Replace("{balance}", balance)} 💵
                    ### Outcome
                    {reason.Outcome} {(won ? "💰" : "💸")}
                    Your balance: {originalCount.ToString(TaylorBotFormats.BoldReadable)} ➡️ {"taypoint".ToQuantity(result.final_count, TaylorBotFormats.BoldReadable)} {(won ? "📈" : "📉")}
                    """);

            if (amountString != null)
            {
                embed
                    .WithUserAsAuthor(author)
                    .WithDescription(
                        $"""
                        {embed.Description}

                        This command is moving to 👉 </risk play:1190786063136993431> 👈 please use it instead 😊
                        """);

            }

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
            """
            A member from your favorite finance Discord server tells you about **a secret underground stock exchange** with exclusive investment opportunities 🔮
            Skeptical but curious, you decide to participate by investing {balance}
            """,
            "To your surprise, the stocks soar, resulting in 2x profits"),
        new(
            """
            A mysterious woman in an alley offers you a chance to invest in an **unknown startup with promises of cutting-edge AI technology** 🤖
            Despite the secrecy, you decide to take the risk and invest {balance}
            """,
            "It pays off as the startup becomes a tech giant, yielding 2x returns"),
        new(
            """
            While browsing a community board, you find a post discussing a **new decentralized cryptocurrency** 🪙
            Intrigued by financial freedom, you invest {balance}
            """,
            "The cryptocurrency gains widespread adoption, leading to 2x profits"),
        new(
            """
            A charismatic guy in a bar shares information about an **upcoming virtual reality gaming platform** 🥽
            Intrigued by immersive gaming experiences, you invest {balance}
            """,
            "The platform becomes a massive success, resulting in 2x profits"),
        new(
            """
            An anonymous message in a chat room hints at a hidden opportunity to invest in **a futuristic energy project** ⚡
            Despite the lack of details, you take a chance and invest {balance}
            """,
            "The project turns out to be a groundbreaking success, securing you 2x gains"),
    ];

    private static readonly Reason[] LoseReasons = [
        new(
            """
            As you stroll through an online marketplace, an anonymous user sends you a message claiming to have **insider information on a groundbreaking stock** 😱
            Tempted by the promise of quick riches, you invest {balance}
            """,
            "It was a **pump-and-dump scheme**, leaving you with worthless shares"),
        new(
            """
            At a finance convention, a charismatic man invites you to invest in a **new cryptocurrency promising unprecedented returns** 🪙
            Intrigued by digital wealth, you invest {balance}
            """,
            "You wake up the next day and find the cryptocurrency has collapsed"),
        new(
            """
            A mysterious email lands in your inbox, offering a chance to invest in a **startup developing revolutionary health supplements** 💊
            Enticed by a potential health breakthrough, you invest {balance}
            """,
            "The company was a front for an elaborate pyramid scheme"),
        new(
            """
            During a business conference, an enthusiastic entrepreneur pitches a **real estate property in a rapidly developing city** 🏚️
            Eager for a slice of the property market, you invest {balance}
            """,
            "The property suddenly collapses, and your investment vanishes"),
        new(
            """
            While exploring a forum, you come across a post about a **rare artifact with the potential to skyrocket in value** 🏺
            Excited by the lucrative collectible, you invest {balance}
            """,
            "The market quickly saturates and the artifact's value plummets"),
    ];

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Play(context, context.User, options.amount));
    }
}
