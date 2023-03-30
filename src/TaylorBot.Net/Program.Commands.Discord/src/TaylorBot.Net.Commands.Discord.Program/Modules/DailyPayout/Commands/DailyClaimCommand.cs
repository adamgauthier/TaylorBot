using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands
{
    public class DailyClaimCommand
    {
        public static readonly CommandMetadata Metadata = new("daily", "Daily Payout 👔", new[] { "dailypayout" });

        private readonly IOptionsMonitor<DailyPayoutOptions> _options;
        private readonly IDailyPayoutRepository _dailyPayoutRepository;
        private readonly IMessageOfTheDayRepository _messageOfTheDayRepository;

        public DailyClaimCommand(IOptionsMonitor<DailyPayoutOptions> options, IDailyPayoutRepository dailyPayoutRepository, IMessageOfTheDayRepository messageOfTheDayRepository)
        {
            _options = options;
            _dailyPayoutRepository = dailyPayoutRepository;
            _messageOfTheDayRepository = messageOfTheDayRepository;
        }

        public Command Claim(IUser user, string commandPrefix, bool isLegacyCommand) => new(
            Metadata,
            async () =>
            {
                var embed = new EmbedBuilder();

                if (isLegacyCommand)
                    embed.WithUserAsAuthor(user);

                var canRedeem = await _dailyPayoutRepository.CanUserRedeemAsync(user);

                if (canRedeem is UserCantRedeem userCantRedeem)
                {
                    return new EmbedResult(embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(string.Join('\n', new[] {
                            "You've already redeemed your daily payout today.",
                            $"You can redeem again <t:{userCantRedeem.CanRedeemAt.ToUnixTimeSeconds()}:R>."
                        }))
                    .Build());
                }

                var payoutAmount = isLegacyCommand ? _options.CurrentValue.LegacyDailyPayoutAmount : _options.CurrentValue.DailyPayoutAmount;
                var redeemResult = await _dailyPayoutRepository.RedeemDailyPayoutAsync(user, payoutAmount);

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
                    .WithDescription(string.Join('\n', new[] {
                        $"You redeemed {"taypoint".ToQuantity(payoutAmount, format)} + {"bonus point".ToQuantity(redeemResult.BonusAmount, format)}." +
                            $" You now have {redeemResult.TotalTaypointCount.ToString(format)}. 💰",
                        $"Bonus streak: {redeemResult.CurrentDailyStreak.ToString(format)}/{nextStreakForBonus.ToString(format)}." +
                            " Don't miss a day and get a bonus! See you tomorrow! 😄",
                        $"\n**Daily Message:** {messageOfTheDay.Replace("{prefix}", commandPrefix)}"
                    }))
                .Build());
            }
        );

        private async Task<string> GetMessageOfTheDayAsync()
        {
            var messages = await _messageOfTheDayRepository.GetAllMessagesAsync();

            var now = DateTimeOffset.UtcNow;

            var messagePriorities = messages.ToLookup(m => m.MessagePriority != null && now >= m.MessagePriority.From && now <= m.MessagePriority.To);
            var priorities = messagePriorities[true].ToList();
            var nonPriorities = messagePriorities[false].ToList();

            var messagesToConsider = priorities.Any() ? priorities : nonPriorities;

            var messageOfTheDay = messagesToConsider[now.DayOfYear % messagesToConsider.Count].Message;

            return messageOfTheDay;
        }
    }

    public class DailyClaimSlashCommand : ISlashCommand<NoOptions>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("daily claim");

        private readonly DailyClaimCommand _dailyClaimCommand;

        public DailyClaimSlashCommand(DailyClaimCommand dailyClaimCommand)
        {
            _dailyClaimCommand = dailyClaimCommand;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
        {
            return new(
                _dailyClaimCommand.Claim(context.User, context.CommandPrefix, isLegacyCommand: false)
            );
        }
    }
}

