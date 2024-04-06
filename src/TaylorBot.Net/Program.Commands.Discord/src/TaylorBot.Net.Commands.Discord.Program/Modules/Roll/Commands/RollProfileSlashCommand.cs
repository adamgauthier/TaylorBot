using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;

public record RollProfile(long roll_count, long perfect_roll_count);

public record RollLeaderboardEntry(string user_id, string username, long perfect_roll_count, long rank);

public interface IRollStatsRepository
{
    Task WinRollAsync(DiscordUser user, long taypointReward);
    Task WinPerfectRollAsync(DiscordUser user, long taypointReward);
    Task AddRollCountAsync(DiscordUser user);
    Task<RollProfile?> GetProfileAsync(DiscordUser user);
    Task<IList<RollLeaderboardEntry>> GetLeaderboardAsync(CommandGuild guild);
}

public class RollProfileSlashCommand(IRollStatsRepository rollStatsRepository) : ISlashCommand<RollProfileSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("roll profile");

    public record Options(ParsedFetchedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = options.user.User;
                var profile = await rollStatsRepository.GetProfileAsync(new(user)) ?? new(0, 0);

                var expectedPerfectRolls = profile.roll_count / 1990;
                var hasPositiveRecord = profile.perfect_roll_count >= expectedPerfectRolls;

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(hasPositiveRecord ? TaylorBotColors.SuccessColor : TaylorBotColors.ErrorColor)
                    .WithTitle($"@{user.Username}'s Roll Profile")
                    .WithThumbnailUrl(user.GetAvatarUrlOrDefault())
                    .WithDescription(
                        $"""
                        Rolled {"time".ToQuantity(profile.roll_count, TaylorBotFormats.BoldReadable)} 🧻
                        Got {"perfect 1989 roll".ToQuantity(profile.perfect_roll_count, TaylorBotFormats.BoldReadable)} 🍀
                        {(hasPositiveRecord ? "🟢" : "🔴")} {(profile.perfect_roll_count > 0
                            ? $"Perfect rate: every {((int)Math.Round((decimal)profile.roll_count / profile.perfect_roll_count)).ToString(TaylorBotFormats.BoldReadable)} rolls"
                            : $"Odds of being this unlucky: **{GetPercentOfNotWinning(profile)}**")}
                        """)
                    .Build());
            }
        ));
    }

    private static string GetPercentOfNotWinning(RollProfile profile)
    {
        var probabilityOfLosing = 1989.0 / 1990;
        var probabilityOfNotWinning = Math.Pow(probabilityOfLosing, profile.roll_count);
        return $"{probabilityOfNotWinning:0%}";
    }
}
