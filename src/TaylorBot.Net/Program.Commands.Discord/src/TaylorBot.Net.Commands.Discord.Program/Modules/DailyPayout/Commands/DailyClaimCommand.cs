using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

public class DailyClaimCommand(IOptionsMonitor<DailyPayoutOptions> options, IDailyPayoutRepository dailyPayoutRepository, IMessageOfTheDayRepository messageOfTheDayRepository)
{
    public static readonly CommandMetadata Metadata = new("daily", "Daily Payout 👔", ["dailypayout"]);

    public Command Claim(DiscordUser user, string commandPrefix, bool isLegacyCommand) => new(
        Metadata,
        async () =>
        {
            EmbedBuilder embed = new();

            var canRedeem = await dailyPayoutRepository.CanUserRedeemAsync(user);

            if (canRedeem is UserCantRedeem userCantRedeem)
            {
                return new EmbedResult(embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription(
                        $"""
                        You've already redeemed your daily payout today.
                        You can redeem again {userCantRedeem.CanRedeemAt.FormatRelative()}.
                        """)
                .Build());
            }

            var payoutAmount = isLegacyCommand ? options.CurrentValue.LegacyDailyPayoutAmount : options.CurrentValue.DailyPayoutAmount;
            var redeemResult = await dailyPayoutRepository.RedeemDailyPayoutAsync(user, payoutAmount);

            if (redeemResult == null)
            {
                return new EmbedResult(embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription("You've already redeemed your daily payout today.")
                .Build());
            }

            var messageOfTheDay = await GetMessageOfTheDayAsync();

            var nextStreakForBonus = redeemResult.CurrentDailyStreak - redeemResult.CurrentDailyStreak % redeemResult.DaysForBonus + redeemResult.DaysForBonus;
            var format = TaylorBotFormats.BoldReadable;

            return new EmbedResult(embed
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(
                    $"""
                    You redeemed {"taypoint".ToQuantity(payoutAmount, format)} + {"bonus point".ToQuantity(redeemResult.BonusAmount, format)}. You now have {redeemResult.TotalTaypointCount.ToString(format)} 💰
                    Streak: {redeemResult.CurrentDailyStreak.ToString(format)}/{nextStreakForBonus.ToString(format)}. Don't miss a day and get a bonus! See you tomorrow! 😄
                    ### Daily Message 📨
                    {messageOfTheDay.Replace("{prefix}", commandPrefix, StringComparison.InvariantCulture)}
                    """)
            .Build());
        }
    );

    private async Task<string> GetMessageOfTheDayAsync()
    {
        var messages = await messageOfTheDayRepository.GetAllMessagesAsync();

        var now = DateTimeOffset.UtcNow;

        var messagePriorities = messages.ToLookup(m => m.MessagePriority != null && now >= m.MessagePriority.From && now <= m.MessagePriority.To);
        var priorities = messagePriorities[true].ToList();
        var nonPriorities = messagePriorities[false].ToList();

        var messagesToConsider = priorities.Count != 0 ? priorities : nonPriorities;

        var messageOfTheDay = messagesToConsider[now.DayOfYear % messagesToConsider.Count].Message;

        return messageOfTheDay;
    }
}

public class DailyClaimSlashCommand(DailyClaimCommand dailyClaimCommand) : ISlashCommand<NoOptions>
{
    public static string CommandName => "daily claim";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public async ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        var prefix = await context.CommandPrefix.Value;
        return dailyClaimCommand.Claim(context.User, prefix, isLegacyCommand: false);
    }
}
