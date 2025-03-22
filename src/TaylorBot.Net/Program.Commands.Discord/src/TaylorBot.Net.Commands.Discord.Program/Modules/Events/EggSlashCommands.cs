using Dapper;
using Discord;
using Humanizer;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events;

public record Egg(string egg_number);

public record EggRarity(string egg_number, long found_by);

public record EggLeaderboardEntry(string user_id, string username, long eggs_found, long rank);

public interface IEggFindAddResult { }
public record EggFindAddedResult() : IEggFindAddResult;
public record EggAlreadyFoundResult() : IEggFindAddResult;

public interface IEggRepository
{
    Task<bool> IsHuntOverAsync();
    Task EndHuntAsync();
    Task<string?> GetConfigAsync(string key);
    Task SetConfigAsync(string key, string value);
    Task<Egg?> GetEggAsync(string code);
    Task<IList<Egg>> GetEggFindsForUserAsync(string userId);
    Task<IList<EggRarity>> GetEggRarityAsync();
    Task<IList<EggLeaderboardEntry>> GetLeaderboardAsync();
    Task<IEggFindAddResult> AddEggFindAsync(string userId, string username, string eggNumber);
}

public class EggPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IEggRepository
{
    public async Task<Egg?> GetEggAsync(string code)
    {
        using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Egg?>(
            "SELECT egg_number FROM egghunt2024.eggs WHERE egg_code = @EggId;",
            new
            {
                EggId = code,
            }
        );
    }

    public async Task<IList<Egg>> GetEggFindsForUserAsync(string userId)
    {
        using var connection = postgresConnectionFactory.CreateConnection();

        var eggs = await connection.QueryAsync<Egg>(
            """
            SELECT eggs.egg_number
            FROM egghunt2024.egg_finds
            INNER JOIN egghunt2024.eggs ON egghunt2024.egg_finds.egg_number = egghunt2024.eggs.egg_number
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = userId,
            }
        );

        return eggs.ToList();
    }

    public async Task<IEggFindAddResult> AddEggFindAsync(string userId, string username, string eggNumber)
    {
        using var connection = postgresConnectionFactory.CreateConnection();

        var inserted = await connection.QuerySingleOrDefaultAsync<bool>(
            """
            INSERT INTO egghunt2024.egg_finds (egg_number, user_id, username)
            VALUES (@EggNumber, @UserId, @Username) ON CONFLICT DO NOTHING
            RETURNING TRUE;
            """,
            new
            {
                EggNumber = eggNumber,
                UserId = userId,
                Username = username,
            }
        );

        return inserted ? new EggFindAddedResult() : new EggAlreadyFoundResult();
    }

    public async Task<bool> IsHuntOverAsync()
    {
        var is_hunt_over = await GetConfigAsync("is_hunt_over");
        return is_hunt_over == "true";
    }

    public Task EndHuntAsync()
    {
        return SetConfigAsync("is_hunt_over", "true");
    }

    public async Task SetConfigAsync(string key, string value)
    {
        using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO egghunt2024.config (config_key, config_value)
            VALUES (@Key, @Value)
            ON CONFLICT (config_key) DO UPDATE SET config_value = @Value;
            """,
            new
            {
                Key = key,
                Value = value,
            }
        );
    }

    public async Task<string?> GetConfigAsync(string key)
    {
        using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<string?>(
            "SELECT config_value FROM egghunt2024.config WHERE config_key = @Key;",
            new
            {
                Key = key,
            }
        );
    }

    public async Task<IList<EggRarity>> GetEggRarityAsync()
    {
        using var connection = postgresConnectionFactory.CreateConnection();

        var eggRarity = await connection.QueryAsync<EggRarity>(
            """
            SELECT e.egg_number, COUNT(ef.egg_number) AS found_by
            FROM egghunt2024.eggs e
            LEFT JOIN egghunt2024.egg_finds ef ON e.egg_number = ef.egg_number
            GROUP BY e.egg_number
            ORDER BY found_by DESC, egg_number ASC;
            """
        );

        return eggRarity.ToList();
    }

    public async Task<IList<EggLeaderboardEntry>> GetLeaderboardAsync()
    {
        using var connection = postgresConnectionFactory.CreateConnection();

        var entries = await connection.QueryAsync<EggLeaderboardEntry>(
            """
            SELECT leaderboard.user_id, u.username, eggs_found, rank() OVER (ORDER BY eggs_found DESC) AS rank
            FROM (
                SELECT ef.user_id, COUNT(ef.egg_number) AS eggs_found
                FROM egghunt2024.egg_finds ef
                JOIN egghunt2024.eggs e ON ef.egg_number = e.egg_number
                GROUP BY ef.user_id
                ORDER BY eggs_found DESC
                LIMIT 75
            ) leaderboard
            JOIN users.users AS u ON leaderboard.user_id = u.user_id;
            """
        );

        return entries.ToList();
    }
}

public class EggService(IEggRepository eggRepository)
{
    public async Task<ICommandResult?> CheckDisabledAsync(RunContext context)
    {
        var vip = await eggRepository.GetConfigAsync("vip_users");
        if (vip != null)
        {
            var userIds = vip.Split(',');
            if (userIds.Any(id => new SnowflakeId(id) == context.User.Id))
            {
                return null;
            }
        }

        var isDisabledInGuild = (await eggRepository.GetConfigAsync($"is_disabled_in_{context.Guild?.Id}"))?.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        if (isDisabledInGuild == true)
        {
            return new EmbedResult(EmbedFactory.CreateError(
                """
                It looks like the hunt is currently disabled 🤔
                Stay tuned for announcements! 👀
                """
            ));
        }
        return null;
    }
}

public class EggVerifySlashCommand(IEggRepository eggRepository, EggService eggService, CommandMentioner mention) : ISlashCommand<EggVerifySlashCommand.Options>
{
    public static string CommandName => "egg verify";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);
    public record Options(ParsedString code);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var disabled = await eggService.CheckDisabledAsync(context);
                if (disabled is not null)
                {
                    return disabled;
                }

                var code = options.code.Value.ToUpperInvariant();

                static ICommandResult InvalidCode()
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Sorry, the code you provided is not a valid egg 🥲

                        Here are some tips:
                        - Egg codes are always **16 characters long**
                        - Egg codes are made up of **uppercase letters** and **numbers**
                        - Egg codes are usually **hidden next to an egg emoji, picture or text**

                        Here's an example of a valid code: `ABCDEFGH12345678` ✅
                        """
                    ));
                }

                if (code.Length != 16)
                {
                    return InvalidCode();
                }

                var egg = await eggRepository.GetEggAsync(code);
                if (egg == null)
                {
                    return InvalidCode();
                }

                if (await eggRepository.IsHuntOverAsync())
                {
                    return new EmbedResult(EmbedFactory.CreateSuccess(
                        $"""
                        Congratulations, you've found 🥚 **#{egg.egg_number}** (`{code}`)! 🎊

                        Your {mention.SlashCommand("egg profile", context)} was not updated because someone finished the hunt! 🏆
                        You can still hunt and verify codes for fun! 😊
                        """));
                }

                var result = await eggRepository.AddEggFindAsync($"{context.User.Id}", context.User.Username, egg.egg_number);
                switch (result)
                {
                    case EggFindAddedResult:
                        var eggs = await eggRepository.GetEggFindsForUserAsync($"{context.User.Id}");
                        if (eggs.Count == 13)
                        {
                            await eggRepository.EndHuntAsync();
                            return new EmbedResult(EmbedFactory.CreateSuccess(
                                $"""
                                ## 🐇🥚🏆 Congratulations!!! 🏆🥚🐇
                                You've found 🥚 **#{egg.egg_number}**, the last egg! 😱
                                Contact Adam and stay tuned for rewards! 🍬
                                """));
                        }
                        else
                        {
                            return new EmbedResult(EmbedFactory.CreateSuccess(
                                $"""
                                Congratulations, you've found 🥚 **#{egg.egg_number}** (`{code}`)! 🎊
                                Your {mention.SlashCommand("egg profile", context)} has been updated! ✅

                                Make sure **all your teammates verify this code as soon as possible** to secure maximum points for your team! 👪
                                """));
                        }

                    case EggAlreadyFoundResult:
                        return new EmbedResult(EmbedFactory.CreateError(
                            $"""
                            Oops, it looks like you submitted the code for 🥚 **#{egg.egg_number}**
                            You've already found this egg! 🤔
                            """
                        ));

                    default: throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
        ));
    }
}

public class EggProfileSlashCommand(IEggRepository eggRepository, EggService eggService, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "egg profile";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var disabled = await eggService.CheckDisabledAsync(context);
                if (disabled is not null)
                {
                    return disabled;
                }

                var eggs = await eggRepository.GetEggFindsForUserAsync($"{context.User.Id}");

                var description = $"You have found {"egg".ToQuantity(eggs.Count, TaylorBotFormats.BoldReadable)} out of 13! 🥚";

                if (eggs.Count > 0)
                {
                    description += $"\n{string.Join("\n", eggs.OrderBy(e => e.egg_number).Select(e => $"- 🥚 **#{e.egg_number}**"))}";
                }

                description += $"\nSee the status of the hunt with {mention.SlashCommand("egg status", context)} 👀";

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle($"@{context.User.Username}'s Egg Profile")
                    .WithThumbnailUrl(context.User.GetAvatarUrlOrDefault())
                    .WithDescription(description)
                    .Build());
            }
        ));
    }
}

public class EggStatusSlashCommand(IEggRepository eggRepository, EggService eggService, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "egg status";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var disabled = await eggService.CheckDisabledAsync(context);
                if (disabled is not null)
                {
                    return disabled;
                }

                var eggs = await eggRepository.GetEggRarityAsync();

                var solvedLookup = eggs.ToLookup(e => e.found_by > 0);
                var solved = solvedLookup[true].ToList();
                var unsolved = solvedLookup[false].ToList();

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        ### Solved Eggs 🕵️
                        {(solved.Count > 0 ?
                            string.Join("\n", solved.Select(e => $"- 🥚 **#{e.egg_number}**: Found by {"hunter".ToQuantity(e.found_by, TaylorBotFormats.BoldReadable)}"))
                            : "None!")}
                        ### Unsolved Eggs 🔮
                        {(unsolved.Count > 0
                            ? string.Join("\n", unsolved.Select(e => $"- 🥚 **#{e.egg_number}**: Found by no one!"))
                            : "None!")}

                        See the leaderboard of hunters with {mention.SlashCommand("egg leaderboard", context)} 👀
                        """)
                    .Build());
            }
        ));
    }
}

public class EggLeaderboardSlashCommand(IEggRepository eggRepository, EggService eggService, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "egg leaderboard";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var disabled = await eggService.CheckDisabledAsync(context);
                if (disabled is not null)
                {
                    return disabled;
                }

                var leaderboard = await eggRepository.GetLeaderboardAsync();

                var pages = leaderboard.Chunk(15).Select(entries =>
                    $"""
                    {string.Join('\n', entries.Select(
                        entry => $"{entry.rank}\\. {entry.username.MdUserLink(entry.user_id)}: {"egg".ToQuantity(entry.eggs_found, TaylorBotFormats.BoldReadable)}"
                    ))}

                    See your own progress with {mention.SlashCommand("egg profile", context)} 👀
                    """).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Egg Hunter Leaderboard 🥚");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                            $"""
                            No eggs found yet! 👀
                            Start hunting and verify your eggs with {mention.SlashCommand("egg verify", context)}! 😊
                            """)),
                    IsCancellable: true
                )).Build();
            }
        ));
    }
}

public class EggSetConfigSlashCommand(IEggRepository eggRepository, TaylorBotOwnerPrecondition ownerPrecondition) : ISlashCommand<EggSetConfigSlashCommand.Options>
{
    public static string CommandName => "owner set-config";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);
    public record Options(ParsedString key, ParsedString value);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await eggRepository.SetConfigAsync(options.key.Value, options.value.Value);
                return new EmbedResult(EmbedFactory.CreateSuccess($"Set {options.key.Value} to {options.value.Value}"));
            },
            Preconditions: [ownerPrecondition]
        ));
    }
}

public class EggRunSlashCommand(PostgresConnectionFactory postgresConnectionFactory, TaylorBotOwnerPrecondition ownerPrecondition) : ISlashCommand<EggRunSlashCommand.Options>
{
    public static string CommandName => "owner run";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);
    public record Options(ParsedString value);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                if (context.User.Id != 119341483219353602)
                {
                    return new EmbedResult(EmbedFactory.CreateError("Unauthorized"));
                }

                if (!options.value.Value.Contains("egg"))
                {
                    return new EmbedResult(EmbedFactory.CreateError("Not egg"));
                }

                using var connection = postgresConnectionFactory.CreateConnection();
                await connection.ExecuteAsync(options.value.Value);

                return new EmbedResult(EmbedFactory.CreateSuccess("Ran"));
            },
            Preconditions: [ownerPrecondition]
        ));
    }
}
