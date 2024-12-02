using Discord;
using Humanizer;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Coupons;

public record RedeemedCoupon(DateTime redeemed_at, string coupon_code, long coupon_reward);

public partial interface ICouponRepository
{
    Task<IList<RedeemedCoupon>> GetRedeemedCouponsAsync(DiscordUser user);
}

public class CouponShowSlashCommand(ICouponRepository couponRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("coupon show", IsPrivateResponse: true);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = context.User;

                var coupons = await couponRepository.GetRedeemedCouponsAsync(user);

                var couponsAsLines = coupons.Select(
                    c => $"{new DateTimeOffset(c.redeemed_at).FormatLongDate()}: {"taypoint".ToQuantity(c.coupon_reward, TaylorBotFormats.BoldReadable)} 🎫 ||{c.coupon_code}||");

                var pages = couponsAsLines.Chunk(size: 15)
                    .Select(lines => string.Join('\n', lines))
                    .ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Redeemed Coupons 🎫");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                            $"""
                            You've never redeemed a coupon before 😵
                            You can get coupon codes from events and redeem them with {context.MentionCommand("coupon redeem")} for special rewards ✨
                            """
                    ))
                )).Build();
            }
        ));
    }
}
