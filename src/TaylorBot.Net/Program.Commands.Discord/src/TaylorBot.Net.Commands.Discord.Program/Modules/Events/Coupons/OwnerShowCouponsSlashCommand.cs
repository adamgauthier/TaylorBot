using Discord;
using Humanizer;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Coupons;

public class OwnerShowCouponsSlashCommand(ICouponRepository couponRepository) : ISlashCommand<OwnerShowCouponsSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("owner showcoupons");

    public record Options(ParsedString codes);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var codes = options.codes.Value.Split(',').Select(i => i.Trim()).ToList();

                List<Coupon> coupons = [];
                foreach (var code in codes)
                {
                    var coupon = await couponRepository.GetCouponAsync(code);
                    ArgumentNullException.ThrowIfNull(coupon);
                    coupons.Add(coupon);
                }

                var couponsAsLines = coupons.Select(
                    c => $"🎫 ||{c.code}|| ({"point".ToQuantity(c.taypoint_reward, TaylorBotFormats.BoldReadable)}): {c.used_count.ToString(TaylorBotFormats.Readable)}/{c.usage_limit?.ToString(TaylorBotFormats.Readable)}");

                var pages = couponsAsLines.Chunk(size: 15)
                    .Select(lines => string.Join('\n', lines))
                    .ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Coupons 🎫");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                            """
                            No coupon data to display 😵
                            """
                    ))
                )).Build();
            },
            Preconditions: [
                new TaylorBotOwnerPrecondition()
            ]
        ));
    }
}
