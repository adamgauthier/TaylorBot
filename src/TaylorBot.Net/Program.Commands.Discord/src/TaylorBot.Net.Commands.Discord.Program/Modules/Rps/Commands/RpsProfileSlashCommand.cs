﻿using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Commands;

public class RpsProfileSlashCommand(IRpsStatsRepository rpsStatsRepository) : ISlashCommand<RpsProfileSlashCommand.Options>
{
    public static string CommandName => "rps profile";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = options.user.User;
                var profile = await rpsStatsRepository.GetProfileAsync(user) ?? new(0, 0, 0);

                var totalGamesPlayed = profile.rps_win_count + profile.rps_draw_count + profile.rps_lose_count;
                var winRate = totalGamesPlayed != 0 ? (decimal)profile.rps_win_count / totalGamesPlayed : 0;
                var hasPositiveRecord = winRate >= 1 / 3;

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(hasPositiveRecord ? TaylorBotColors.SuccessColor : TaylorBotColors.ErrorColor)
                    .WithTitle($"@{user.Username}'s Rock Paper Scissors")
                    .WithThumbnailUrl(user.GetAvatarUrlOrDefault())
                    .WithDescription(
                        $"""
                        {(hasPositiveRecord ? "🟢" : "🔴")} **{winRate:0%}** wins out of {"game".ToQuantity(totalGamesPlayed, TaylorBotFormats.BoldReadable)} played
                        Won {"game".ToQuantity(profile.rps_win_count, TaylorBotFormats.BoldReadable)} 😀
                        Drew {"game".ToQuantity(profile.rps_draw_count, TaylorBotFormats.BoldReadable)} 😐
                        Lost {"game".ToQuantity(profile.rps_lose_count, TaylorBotFormats.BoldReadable)} ☹️
                        """)
                    .Build());
            }
        ));
    }
}
