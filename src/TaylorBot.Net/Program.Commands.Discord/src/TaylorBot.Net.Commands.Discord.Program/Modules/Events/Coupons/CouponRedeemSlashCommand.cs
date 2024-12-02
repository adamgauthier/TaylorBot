using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Coupons;

public record Coupon(Guid coupon_id, string code, DateTime valid_from, DateTime valid_until, int? usage_limit, int used_count, long taypoint_reward);

public partial interface ICouponRepository
{
    Task<Coupon?> GetCouponAsync(string code);
    Task<TaypointAddResult?> RedeemCouponAsync(DiscordUser user, Coupon coupon);
}

public class CouponRedeemSlashCommand(IRateLimiter rateLimiter, ICouponRepository couponRepository) : ISlashCommand<CouponRedeemSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("coupon redeem", IsPrivateResponse: true);

    public record Options(ParsedString code);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = context.User;

                var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(user, "redeem-coupon");
                if (rateLimitResult != null)
                    return rateLimitResult;

                var now = DateTime.UtcNow;

                var coupon = await couponRepository.GetCouponAsync(options.code.Value);
                if (coupon == null || now <= coupon.valid_from)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Oops, this coupon code is not valid, sorry 😕
                        Did you make a typo in the code? 🤔
                        """));
                }

                if (now >= coupon.valid_until)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Oops, this coupon is expired 😕
                        Sorry, better luck next time! 👉
                        """));
                }

                if (coupon.used_count >= coupon.usage_limit)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Oops, this coupon has reached the maximum amount of redemptions 😕
                        Sorry, better luck next time! 👉
                        """));
                }

                var createdAt = SnowflakeUtils.FromSnowflake(user.Id);
                if (createdAt > coupon.valid_from)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Oops, it looks like you can't redeem this coupon because your account is too new 😕
                        Sorry! 🙇
                        """));
                }

                var taypointAddResult = await couponRepository.RedeemCouponAsync(user, coupon);
                if (taypointAddResult == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Oops, it looks like you already redeemed this coupon 😕
                        Coupons can only be redeemed once! 🙏
                        """));
                }

                var format = TaylorBotFormats.BoldReadable;
                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Coupon successfully redeemed for {"taypoint".ToQuantity(coupon.taypoint_reward, format)}! ✅
                    You now have {taypointAddResult.taypoint_count.ToString(format)} 💰
                    """));
            }
        ));
    }
}
