using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class TaypointsGiftSlashCommand(ITaypointTransferRepository taypointTransferRepository) : ISlashCommand<TaypointsGiftSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("taypoints gift");

    public record Options(ITaypointAmount amount, ParsedUserNotAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var from = context.User;
                var to = options.user.User;
                var amount = options.amount;

                if (to.IsBot)
                {
                    var amountText = FormatAmount(amount);

                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to transfer {amountText} to a bot? ⚠️
                            Bots can **NOT** transfer taypoints back. **Your taypoints will be lost!** 🥶
                            """)),
                        confirm: () => TransferAsync(from, to, amount)
                    );
                }
                else
                {
                    var shouldConfirm = amount is AbsoluteTaypointAmount absolute
                        ? absolute.Balance >= 1_000 && GetPercent(absolute) >= 0.50
                        : ((RelativeTaypointAmount)amount).Proportion <= 2;

                    if (shouldConfirm)
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
                            confirm: () => TransferAsync(from, to, amount)
                        );
                    }
                    else
                    {
                        return new MessageResult(await TransferAsync(from, to, amount));
                    }
                }
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
            }
        ));
    }

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

    private async ValueTask<MessageContent> TransferAsync(IUser from, IUser to, ITaypointAmount amount)
    {
        var transferResult = await taypointTransferRepository.TransferTaypointsAsync(from, to, amount);

        return new MessageContent(EmbedFactory.CreateSuccess(
            $"""
            ### Taypoint Transfer
            {from.Mention} 🎁 **{"taypoint".ToQuantity(transferResult.GiftedCount, TaylorBotFormats.Readable)}** ➡️ {to.Mention}
            ### Balances Updated
            {from.Mention}: {transferResult.OriginalCount.ToString(TaylorBotFormats.BoldReadable)} ➡️ {"taypoint".ToQuantity(transferResult.OriginalCount - transferResult.GiftedCount, TaylorBotFormats.BoldReadable)} 📉
            {to.Mention}: {(transferResult.ReceiverNewCount - transferResult.GiftedCount).ToString(TaylorBotFormats.BoldReadable)} ➡️ {"taypoint".ToQuantity(transferResult.ReceiverNewCount, TaylorBotFormats.BoldReadable)} 📈
            """));
    }
}

