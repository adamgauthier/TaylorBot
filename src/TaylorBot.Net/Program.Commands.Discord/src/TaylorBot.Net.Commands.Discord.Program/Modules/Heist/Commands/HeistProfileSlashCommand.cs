using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class HeistProfileSlashCommand(IHeistStatsRepository heistStatsRepository) : ISlashCommand<HeistProfileSlashCommand.Options>
{
    public static string CommandName => "heist profile";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = options.user.User;
                var profile = (await heistStatsRepository.GetProfileAsync(user)) ?? new(0, 0, 0, 0);

                var totalHeistPlayed = profile.heist_win_count + profile.heist_lose_count;
                var winRate = totalHeistPlayed != 0 ? (decimal)profile.heist_win_count / totalHeistPlayed : 0;
                var hasPositiveRecord = winRate >= (decimal)0.5;
                var profits = profile.heist_win_amount - profile.heist_lose_amount;

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(hasPositiveRecord ? TaylorBotColors.SuccessColor : TaylorBotColors.ErrorColor)
                    .WithTitle($"@{user.Username}'s Heist Profile")
                    .WithThumbnailUrl(user.GetAvatarUrlOrDefault())
                    .AddField("Heists Won", $"{(hasPositiveRecord ? "🟢" : "🔴")} **{winRate:0%}** out of {"total heist".ToQuantity(totalHeistPlayed, TaylorBotFormats.BoldReadable)} played")
                    .AddField("Heist Profits",
                        $"""
                        **{(profits >= 0 ? "🟢 +" : "🔴 —")}{"taypoint".ToQuantity(Math.Abs(profits), TaylorBotFormats.Readable)}**
                        Won {"taypoint".ToQuantity(profile.heist_win_amount, TaylorBotFormats.BoldReadable)} 📈
                        Lost {"taypoint".ToQuantity(profile.heist_lose_amount, TaylorBotFormats.BoldReadable)} 📉
                        """)
                    .Build());
            }
        ));
    }
}
