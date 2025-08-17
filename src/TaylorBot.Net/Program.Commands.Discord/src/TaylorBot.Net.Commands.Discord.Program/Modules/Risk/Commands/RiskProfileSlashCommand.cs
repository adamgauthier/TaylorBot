﻿using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;

public class RiskProfileSlashCommand(IRiskStatsRepository riskStatsRepository) : ISlashCommand<RiskProfileSlashCommand.Options>
{
    public static string CommandName => "risk profile";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = options.user.User;
                var profile = (await riskStatsRepository.GetProfileAsync(user)) ?? new(0, 0, 0, 0);

                var totalRiskPlayed = profile.risk_win_count + profile.risk_lose_count;
                var winRate = totalRiskPlayed != 0 ? (decimal)profile.risk_win_count / totalRiskPlayed : 0;
                var hasPositiveRecord = winRate >= (decimal)0.5;
                var profits = profile.risk_win_amount - profile.risk_lose_amount;

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(hasPositiveRecord ? TaylorBotColors.SuccessColor : TaylorBotColors.ErrorColor)
                    .WithTitle($"@{user.Username}'s Risk Profile")
                    .WithThumbnailUrl(user.GetAvatarUrlOrDefault())
                    .AddField("Risks Won", $"{(hasPositiveRecord ? "🟢" : "🔴")} **{winRate:0%}** out of {"total risk".ToQuantity(totalRiskPlayed, TaylorBotFormats.BoldReadable)} played")
                    .AddField("Risk Profits",
                        $"""
                        **{(profits >= 0 ? "🟢 +" : "🔴 —")}{"taypoint".ToQuantity(Math.Abs(profits), TaylorBotFormats.Readable)}**
                        Won {"taypoint".ToQuantity(profile.risk_win_amount, TaylorBotFormats.BoldReadable)} 📈
                        Lost {"taypoint".ToQuantity(profile.risk_lose_amount, TaylorBotFormats.BoldReadable)} 📉
                        """)
                    .Build());
            }
        ));
    }
}
