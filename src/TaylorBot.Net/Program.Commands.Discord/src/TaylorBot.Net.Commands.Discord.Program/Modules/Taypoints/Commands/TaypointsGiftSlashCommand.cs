using Discord;
using Humanizer;
using System.Globalization;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class TaypointsGiftSlashCommand : ISlashCommand<TaypointsGiftSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("taypoints gift");

    public record Options(ParsedPositiveInteger amount, ParsedUserNotAuthor user);

    private readonly ITaypointTransferRepository _taypointTransferRepository;
    private readonly ITaypointBalanceRepository _taypointBalanceRepository;

    public TaypointsGiftSlashCommand(ITaypointTransferRepository taypointTransferRepository, ITaypointBalanceRepository taypointBalanceRepository)
    {
        _taypointTransferRepository = taypointTransferRepository;
        _taypointBalanceRepository = taypointBalanceRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var from = context.User;
                var to = options.user.User;
                var amount = options.amount.Value;

                var balance = await _taypointBalanceRepository.GetBalanceAsync(from);

                if (amount > balance.TaypointCount)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"You can't spend {"taypoint".ToQuantity(amount, TaylorBotFormats.BoldReadable)}, you only have {balance.TaypointCount.ToString(TaylorBotFormats.BoldReadable)}. 😕"));
                }

                if (to.IsBot)
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to transfer {"taypoint".ToQuantity(amount, TaylorBotFormats.BoldReadable)} to a bot? ⚠️
                            Bots can **NOT** transfer taypoints back. **Your taypoints will be lost!** 🥶
                            """)),
                        confirm: () => TransferAsync(from, to, amount)
                    );
                }
                else
                {
                    var percent = balance.TaypointCount != 0 ? (double)amount / balance.TaypointCount : 0;

                    if (balance.TaypointCount >= 1_000 && percent >= 0.50)
                    {
                        percent.ToString("0%", CultureInfo.InvariantCulture);

                        return MessageResult.CreatePrompt(
                            new(EmbedFactory.CreateWarning(
                                $"""
                                Are you sure you want to transfer {"taypoint".ToQuantity(amount, TaylorBotFormats.BoldReadable)}? ⚠️
                                This represents **{percent:0%}** of your balance of {balance.TaypointCount.ToString(TaylorBotFormats.BoldReadable)} 💰
                                """)),
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

    private async ValueTask<MessageContent> TransferAsync(IUser from, IUser to, int amount)
    {
        var transferResult = await _taypointTransferRepository.TransferTaypointsAsync(from, to, amount);

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

