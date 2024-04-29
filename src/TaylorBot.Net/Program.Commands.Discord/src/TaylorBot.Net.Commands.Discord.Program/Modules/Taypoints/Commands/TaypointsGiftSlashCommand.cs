using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class TaypointsGiftSlashCommand(
    ITaypointTransferRepository taypointTransferRepository, TaypointAmountParser amountParser, TaypointGuildCacheUpdater taypointGuildCacheUpdater) : ISlashCommand<TaypointsGiftSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("taypoints gift");

    public record Options(ITaypointAmount amount, ParsedUserNotAuthor user);

    public Command Gift(RunContext context, IReadOnlyList<DiscordUser> recipients, ITaypointAmount? amount, string? amountString = null) => new(
        new(Info.Name),
        async () =>
        {
            if (amountString != null)
            {
                var parsed = await amountParser.ParseStringAsync(context, amountString);
                if (!parsed)
                {
                    return new EmbedResult(EmbedFactory.CreateError($"`amount`: {parsed.Error.Message}"));
                }
                amount = parsed.Value;
            }
            ArgumentNullException.ThrowIfNull(amount);

            var author = context.User;

            if (amountString == null && recipients.Any(r => r.IsBot))
            {
                var amountText = FormatAmount(amount);

                return MessageResult.CreatePrompt(
                    new(EmbedFactory.CreateWarning(
                        $"""
                        Are you sure you want to transfer {amountText} to a bot? ⚠️
                        Bots can **NOT** transfer taypoints back. **Your taypoints will be lost!** 🥶
                        """)),
                    confirm: async () => new(EmbedFactory.CreateSuccess(await TransferAsync(context, author, recipients, amount)))
                );
            }
            else
            {
                var shouldConfirm = amount is AbsoluteTaypointAmount absolute
                    ? absolute.Balance >= 1_000 && GetPercent(absolute) >= 0.50
                    : ((RelativeTaypointAmount)amount).Proportion <= 2;

                if (amountString == null && shouldConfirm)
                {
                    var amountText = FormatAmount(amount);
                    var promptText = amount is AbsoluteTaypointAmount absolute2
                        ? $"""
                          Are you sure you want to transfer {amountText}? ⚠️
                          This represents **{GetPercent(absolute2):0%}** of your balance of {absolute2.Balance.ToString(TaylorBotFormats.BoldReadable)} 💰
                          """
                        : $"Are you sure you want to transfer {amountText}? ⚠️";

                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(promptText)),
                        confirm: async () => new(EmbedFactory.CreateSuccess(await TransferAsync(context, author, recipients, amount)))
                    );
                }
                else
                {
                    var description = await TransferAsync(context, author, recipients, amount);
                    if (amountString != null)
                    {
                        description += "\n\nCheck out </taypoints gift:1103846727880028180>!";
                    }
                    return new EmbedResult(EmbedFactory.CreateSuccess(description.Truncate(EmbedBuilder.MaxDescriptionLength)));
                }
            }
        }
    );

    private static double GetPercent(AbsoluteTaypointAmount absolute)
    {
        return absolute.Balance != 0 ? (double)absolute.Amount / absolute.Balance : 0;
    }

    private static string FormatAmount(ITaypointAmount amount)
    {
        return amount is AbsoluteTaypointAmount absolute
            ? "taypoint".ToQuantity(absolute.Amount, TaylorBotFormats.BoldReadable)
            : FormatRelativeAmount((RelativeTaypointAmount)amount);
    }

    private static string FormatRelativeAmount(RelativeTaypointAmount amount)
    {
        return amount.Proportion switch
        {
            1 => "all your taypoints",
            2 => "half of your taypoints",
            3 => "a third of your taypoints",
            4 => "a fourth of your taypoints",
            _ => throw new NotImplementedException(),
        };
    }

    private async ValueTask<string> TransferAsync(RunContext context, DiscordUser from, IReadOnlyList<DiscordUser> to, ITaypointAmount amount)
    {
        var transfer = await taypointTransferRepository.TransferTaypointsAsync(from, to, amount);

        var fromBalance = transfer.OriginalCount - transfer.GiftedCount;

        var recipientBalances = transfer.Recipients.Select(r =>
            $"<@{r.UserId}>: {(r.UpdatedBalance - r.Received).ToString(TaylorBotFormats.BoldReadable)} ➡️ {"taypoint".ToQuantity(r.UpdatedBalance, TaylorBotFormats.BoldReadable)} 📈");

        if (context.Guild?.Fetched != null)
        {
            var nonBots = to.Where(u => !u.IsBot);
            var nonBotRecipientResults = transfer.Recipients.Where(r => nonBots.Any(u => $"{u.Id}" == r.UserId));

            var updates = nonBotRecipientResults
                .Select(r => new TaypointCountUpdate(r.UserId, r.UpdatedBalance))
                .Append(new(from.Id, fromBalance))
                .ToList();
            taypointGuildCacheUpdater.UpdateLastKnownPointCountsInBackground(context.Guild, updates);
        }

        return
            $"""
            ### Taypoint Transfer
            {from.Mention} 🎁 **{"taypoint".ToQuantity(transfer.GiftedCount, TaylorBotFormats.Readable)}** ➡️ {(to.Count > 1 ? "__multiple users__" : to[0].Mention)}
            ### Balances Updated
            {from.Mention}: {transfer.OriginalCount.ToString(TaylorBotFormats.BoldReadable)} ➡️ {"taypoint".ToQuantity(fromBalance, TaylorBotFormats.BoldReadable)} 📉
            {string.Join('\n', recipientBalances)}            
            """;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Gift(context, [options.user.User], options.amount));
    }
}

