using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

public class DailyClaimSlashCommand(
    IOptionsMonitor<DailyPayoutOptions> options,
    IDailyPayoutRepository dailyPayoutRepository,
    IMessageOfTheDayRepository messageOfTheDayRepository,
    TimeProvider timeProvider,
    CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "daily claim";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public static readonly CommandMetadata Metadata = new("daily", ["dailypayout"]);

    public Command Claim(DiscordUser user, RunContext context) => new(
        context.SlashCommand != null ? Metadata : Metadata with { IsSlashCommand = false },
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

            var payoutAmount = context.SlashCommand == null ? options.CurrentValue.LegacyDailyPayoutAmount : options.CurrentValue.DailyPayoutAmount;
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

            var bold = TaylorBotFormats.BoldReadable;
            string BoldReadable(long num) => num.ToString(bold, TaylorBotCulture.Culture);
            string BoldQuantity(long num, string word) => word.ToQuantity(num, bold, TaylorBotCulture.Culture);

            return new EmbedResult(embed
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(
                    $"""
                    You redeemed {BoldQuantity(payoutAmount, "taypoint")} + {BoldQuantity(redeemResult.BonusAmount, "bonus point")}. You now have {BoldReadable(redeemResult.TotalTaypointCount)} 💰
                    Streak: {BoldReadable(redeemResult.CurrentDailyStreak)}/{BoldReadable(nextStreakForBonus)}. Don't miss a day and get a bonus! See you tomorrow! 😄
                    ### Daily Message 📨
                    {messageOfTheDay}
                    """)
            .Build());
        }
    );

    private async Task<string> GetMessageOfTheDayAsync()
    {
        var defaultMessages = DailyMessages.Default;
        var databaseMessages = await messageOfTheDayRepository.GetAllMessagesAsync();

        // Treat database messages as additional messages or overrides for default messages
        var allMessages = defaultMessages.ToDictionary(m => m.Id);
        foreach (var dbMessage in databaseMessages)
        {
            allMessages[dbMessage.Id] = dbMessage;
        }

        var now = timeProvider.GetUtcNow();

        var messagePriorities = allMessages.Values.ToLookup(m => m.MessagePriority != null && now >= m.MessagePriority.From && now <= m.MessagePriority.To);
        var priorities = messagePriorities[true].ToList();
        var nonPriorities = messagePriorities[false].ToList();

        var messagesToConsider = priorities.Count != 0 ? priorities : nonPriorities;

        var messageOfTheDay = messagesToConsider[now.DayOfYear % messagesToConsider.Count].Message;

        return mention.ReplaceSlashCommandMentions(messageOfTheDay);
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(Claim(context.User, context));
    }
}
