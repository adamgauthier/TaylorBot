using Dapper;
using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2024;

public static class Valentines
{
    public static readonly IList<SnowflakeId> VipUserIds = [748723450511753298, 119341483219353602];
}

public class ValentinesVerifySlashCommand(
    PostgresConnectionFactory postgresConnectionFactory,
    CommandMentioner mention) : ISlashCommand<ValentinesVerifySlashCommand.Options>
{
    public static string CommandName => "valentines verify";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public record Options(ParsedString puzzle, ParsedString code);

    private sealed record Code(string puzzle_code, bool enabled);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                using var connection = postgresConnectionFactory.CreateConnection();

                var alreadySolved = await connection.QuerySingleOrDefaultAsync<bool>(
                    "SELECT solved_at IS NOT NULL FROM valentines2024.puzzle_solves WHERE puzzle_id = @PuzzleId AND user_id = @UserId;",
                    new
                    {
                        PuzzleId = options.puzzle.Value,
                        UserId = $"{context.User.Id}",
                    }
                );

                if (alreadySolved)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        You've already completed the {options.puzzle.Value} puzzle!
                        Did you choose the wrong puzzle?
                        """
                    ));
                }

                var code = await connection.QuerySingleAsync<Code>(
                    "SELECT puzzle_code, enabled FROM valentines2024.codes WHERE puzzle_id = @PuzzleId;",
                    new
                    {
                        PuzzleId = options.puzzle.Value,
                    }
                );

                if (!code.enabled && !Valentines.VipUserIds.Contains(context.User.Id))
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        Puzzle {options.puzzle.Value} is not available yet!
                        Did you choose the wrong puzzle?
                        """
                    ));
                }

                if (!code.puzzle_code.Equals(options.code.Value, StringComparison.OrdinalIgnoreCase))
                {
                    await connection.ExecuteAsync(
                        """
                        INSERT INTO valentines2024.puzzle_solves (puzzle_id, user_id, username, solved_at, attempt_count)
                        VALUES (@PuzzleId, @UserId, @Username, NULL, 1)
                        ON CONFLICT (puzzle_id, user_id) DO UPDATE SET
                            attempt_count = valentines2024.puzzle_solves.attempt_count + 1;
                        """,
                        new
                        {
                            PuzzleId = options.puzzle.Value,
                            UserId = $"{context.User.Id}",
                            Username = $"{context.User.Username}",
                        }
                    );

                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        Your code is **NOT** the answer. Added 1 failed attempt to your {mention.GuildSlashCommand("valentines profile", context.Guild?.Id ?? throw new InvalidOperationException())}
                        Better luck next time! 🤐
                        """
                    ));
                }
                else
                {
                    var attemptCount = await connection.QuerySingleAsync<short>(
                        """
                        INSERT INTO valentines2024.puzzle_solves (puzzle_id, user_id, username, solved_at, attempt_count)
                        VALUES (@PuzzleId, @UserId, @Username, NOW(), 1)
                        ON CONFLICT (puzzle_id, user_id) DO UPDATE SET
                            solved_at = NOW()
                        RETURNING attempt_count;
                        """,
                        new
                        {
                            PuzzleId = options.puzzle.Value,
                            UserId = $"{context.User.Id}",
                            Username = $"{context.User.Username}",
                        }
                    );

                    return new EmbedResult(EmbedFactory.CreateSuccess(
                        $"""
                        Congrats, your code is right! 🎉
                        You solved {options.puzzle.Value} after {"attempt".ToQuantity(attemptCount, TaylorBotFormats.BoldReadable)} 🎊
                        Your {mention.GuildSlashCommand("valentines profile", context.Guild?.Id ?? throw new InvalidOperationException())} has been updated! ✅
                        **IMPORTANT**: Make sure all your teammates verify this code as soon as possible to secure maximum points for your team!
                        """
                    ));
                }
            }
        ));
    }
}

public class ValentinesProfileSlashCommand(PostgresConnectionFactory postgresConnectionFactory) : ISlashCommand<NoOptions>
{
    public static string CommandName => "valentines profile";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    private sealed record Solved(string puzzle_id, short attempt_count, DateTime? solved_at);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                using var connection = postgresConnectionFactory.CreateConnection();

                var solved = await connection.QueryAsync<Solved>(
                    """
                    SELECT solves.puzzle_id, attempt_count, solved_at
                    FROM valentines2024.puzzle_solves solves INNER JOIN valentines2024.codes
                    ON solves.puzzle_id = valentines2024.codes.puzzle_id
                    WHERE user_id = @UserId;
                    """,
                    new
                    {
                        UserId = $"{context.User.Id}",
                    }
                );

                var puzzleFearless = solved.SingleOrDefault(s => s.puzzle_id == "fearless");
                var puzzleSpeakNow = solved.SingleOrDefault(s => s.puzzle_id == "speaknow");
                var puzzleRed = solved.SingleOrDefault(s => s.puzzle_id == "red");
                var puzzle1989 = solved.SingleOrDefault(s => s.puzzle_id == "1989");

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle($"@{context.User.Username}'s Valentines Puzzle")
                    .WithThumbnailUrl(context.User.GetAvatarUrlOrDefault())
                    .WithDescription(
                        $"""
                        {(puzzleFearless?.solved_at != null ? "🟢" : "🔴")} Fearless Puzzle 💛 ({"attempt".ToQuantity(puzzleFearless?.attempt_count ?? 0, TaylorBotFormats.BoldReadable)})
                        {(puzzleSpeakNow?.solved_at != null ? "🟢" : "🔴")} Speak Now Puzzle 💜 ({"attempt".ToQuantity(puzzleSpeakNow?.attempt_count ?? 0, TaylorBotFormats.BoldReadable)})
                        {(puzzleRed?.solved_at != null ? "🟢" : "🔴")} Red Puzzle 💓 ({"attempt".ToQuantity(puzzleRed?.attempt_count ?? 0, TaylorBotFormats.BoldReadable)})
                        {(puzzle1989?.solved_at != null ? "🟢" : "🔴")} 1989 Puzzle 💙 ({"attempt".ToQuantity(puzzle1989?.attempt_count ?? 0, TaylorBotFormats.BoldReadable)})
                        """)
                    .Build());
            }
        ));
    }
}

public class ValentinesStatusSlashCommand(PostgresConnectionFactory postgresConnectionFactory) : ISlashCommand<NoOptions>
{
    public static string CommandName => "valentines status";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    private sealed record PuzzleStatus(string puzzle_id, long found_by);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                using var connection = postgresConnectionFactory.CreateConnection();

                var status = (await connection.QueryAsync<PuzzleStatus>(
                    """
                    SELECT s.puzzle_id, cnt AS found_by
                    FROM (
                        SELECT puzzle_id, cnt, RANK() OVER (PARTITION BY puzzle_id ORDER BY cnt DESC) AS rn
                        FROM (
                            SELECT puzzle_id, COUNT(puzzle_id) AS cnt
                            FROM valentines2024.puzzle_solves
                            WHERE solved_at IS NOT NULL
                            GROUP BY puzzle_id) t
                    ) s INNER JOIN valentines2024.codes ON valentines2024.codes.puzzle_id = s.puzzle_id
                    WHERE s.rn = 1
                    ORDER BY cnt DESC;
                    """)).ToList();

                if (status.Count == 0)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        "No one has solved a puzzle yet!"
                    ));
                }

                var puzzleFearless = status.SingleOrDefault(s => s.puzzle_id == "fearless");
                var puzzleSpeakNow = status.SingleOrDefault(s => s.puzzle_id == "speaknow");
                var puzzleRed = status.SingleOrDefault(s => s.puzzle_id == "red");
                var puzzle1989 = status.SingleOrDefault(s => s.puzzle_id == "1989");

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Valentines Puzzle Global Status")
                    .WithDescription(
                        $"""
                        💛 Fearless Puzzle: {(puzzleFearless != null ? $"🕵️ **Solved by {"user".ToQuantity(puzzleFearless.found_by, TaylorBotFormats.Readable)}**" : "🔮 **Unsolved**")}
                        💜 Speak Now Puzzle: {(puzzleSpeakNow != null ? $"🕵️ **Solved by {"user".ToQuantity(puzzleSpeakNow.found_by, TaylorBotFormats.Readable)}**" : "🔮 **Unsolved**")}
                        💓 Red Puzzle: {(puzzleRed != null ? $"🕵️ **Solved by {"user".ToQuantity(puzzleRed.found_by, TaylorBotFormats.Readable)}** " : "🔮 **Unsolved**")}
                        💙 1989 Puzzle: {(puzzle1989 != null ? $"🕵️ **Solved by {"user".ToQuantity(puzzle1989.found_by, TaylorBotFormats.Readable)}**" : "🔮 **Unsolved**")}
                        """)
                    .Build());
            }
        ));
    }
}

public class ValentinesEnableSlashCommand(
    PostgresConnectionFactory postgresConnectionFactory,
    TaylorBotOwnerPrecondition ownerPrecondition) : ISlashCommand<ValentinesEnableSlashCommand.Options>
{
    public static string CommandName => "owner enable-puzzle";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public record Options(ParsedString puzzle, ParsedOptionalBoolean enable);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var enabled = options.enable.Value ?? true;

                using var connection = postgresConnectionFactory.CreateConnection();

                await connection.ExecuteAsync(
                    "UPDATE valentines2024.codes SET enabled = @Enabled WHERE puzzle_id = @PuzzleId;",
                    new
                    {
                        Enabled = enabled,
                        PuzzleId = options.puzzle.Value,
                    }
                );

                return new EmbedResult(EmbedFactory.CreateSuccess($"Set {options.puzzle.Value} Enabled={enabled}"));
            },
            Preconditions: [new TaylorBotOwnerOrIdPrecondition(Valentines.VipUserIds, ownerPrecondition)]));
    }
}

public class TaylorBotOwnerOrIdPrecondition(IList<SnowflakeId> userIds, TaylorBotOwnerPrecondition ownerPrecondition) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (userIds.Contains(context.User.Id))
        {
            return new PreconditionPassed();
        }

        return await ownerPrecondition.CanRunAsync(command, context);
    }
}
