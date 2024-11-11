using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Random;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Commands;

public record RpsProfile(int rps_win_count, int rps_draw_count, int rps_lose_count);

public record RpsLeaderboardEntry(string user_id, string username, int rps_win_count, long rank);

public interface IRpsStatsRepository
{
    Task<RpsProfile?> GetProfileAsync(DiscordUser user);
    Task WinRpsAsync(DiscordUser user, long taypointReward);
    Task DrawRpsAsync(DiscordUser user);
    Task LoseRpsAsync(DiscordUser user);
    Task<IList<RpsLeaderboardEntry>> GetLeaderboardAsync(CommandGuild guild);
}

public class RpsPlaySlashCommand(IRpsStatsRepository rpsStatsRepository, IRateLimiter rateLimiter, ICryptoSecureRandom cryptoSecureRandom) : ISlashCommand<RpsPlaySlashCommand.Options>
{
    public const string PrefixCommandName = "rps";

    public ISlashCommandInfo Info => new MessageCommandInfo("rps play");

    public record Options(RpsShape? option);

    private static readonly List<RpsShape> Shapes = Enum.GetValues(typeof(RpsShape)).Cast<RpsShape>().ToList();

    public Command Play(RunContext context, RpsShape? shape, string? shapeString = null) => new(
        new(Info.Name, Aliases: [PrefixCommandName]),
        async () =>
        {
            if (shapeString != null)
            {
                var parsed = OptionalRpsShapeParser.Parse(shapeString);
                if (!parsed)
                {
                    return new EmbedResult(EmbedFactory.CreateError($"`option`: {parsed.Error.Message}"));
                }
                shape = parsed.Value;
            }

            var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(context.User, "rps");
            if (rateLimitResult != null)
                return rateLimitResult;

            var player = shape ?? cryptoSecureRandom.GetRandomElement(Shapes);
            var opponent = cryptoSecureRandom.GetRandomElement(Shapes);

            var winner = FindWinner(player, opponent);

            var embed = new EmbedBuilder();

            long winReward = AnniversaryEvent.IsActive ? 2 : 1;
            string resultMessage;
            if (winner == player)
            {
                embed.WithColor(TaylorBotColors.SuccessColor);
                await rpsStatsRepository.WinRpsAsync(context.User, winReward);
                resultMessage =
                    $"""
                    {MapWinnerToString(winner.Value)}... **You win!** 😭
                    Here's {"taypoint".ToQuantity(winReward, TaylorBotFormats.BoldReadable)} as a reward 🍬
                    """;
            }
            else if (winner == opponent)
            {
                embed.WithColor(TaylorBotColors.ErrorColor);
                await rpsStatsRepository.LoseRpsAsync(context.User);
                resultMessage =
                    $"""
                    {MapWinnerToString(winner.Value)}... **You lost!** 🤭
                    Better luck next time! 🍀
                    """;
            }
            else
            {
                embed.WithColor(TaylorBotColors.WarningColor);
                await rpsStatsRepository.DrawRpsAsync(context.User);
                resultMessage =
                    $"""
                    We both picked {player.ToString().ToLowerInvariant()}... **It's a tie!** 😶
                    It's like our minds are connected 🧠
                    """;
            }

            return new EmbedResult(embed
                .WithTitle("Rock, paper, scissors!")
                .WithDescription(
                    $"""
                    ### You {MapShapeToString(player)} ➡️ ⚡💥⚡ ⬅️ {MapShapeToString(opponent)} TaylorBot
                    {resultMessage}
                    """)
                .Build());
        }
    );

    private static RpsShape? FindWinner(RpsShape shape1, RpsShape shape2)
    {
        if (shape1 == shape2)
            return null;

        var shapes = new[] { shape1, shape2 };

        if (shapes.Contains(RpsShape.Rock) && shapes.Contains(RpsShape.Paper))
        {
            return RpsShape.Paper;
        }
        if (shapes.Contains(RpsShape.Paper) && shapes.Contains(RpsShape.Scissors))
        {
            return RpsShape.Scissors;
        }
        if (shapes.Contains(RpsShape.Rock) && shapes.Contains(RpsShape.Scissors))
        {
            return RpsShape.Rock;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static string MapWinnerToString(RpsShape shape)
    {
        return shape switch
        {
            RpsShape.Rock => "Rock beats scissors",
            RpsShape.Paper => "Paper beats rock",
            RpsShape.Scissors => "Scissors beats paper",
            _ => throw new NotImplementedException(),
        };
    }

    private static string MapShapeToString(RpsShape shape)
    {
        return shape switch
        {
            RpsShape.Rock => "🪨",
            RpsShape.Paper => "📄",
            RpsShape.Scissors => "✂️",
            _ => throw new NotImplementedException(),
        };
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Play(context, options.option));
    }
}
