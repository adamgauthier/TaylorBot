using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Coupons;

public partial interface ICouponRepository
{
    Task<Guid> AddCouponAsync(Coupon coupon);
}

public class OwnerAddCouponSlashCommand(ICouponRepository couponRepository) : ISlashCommand<OwnerAddCouponSlashCommand.Options>
{
    public static string CommandName => "owner addcoupon";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString code, ParsedTimeSpan from, ParsedTimeSpan until, ParsedPositiveInteger limit, ParsedPositiveInteger reward);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                Coupon coupon = new(
                    coupon_id: Guid.Empty, // Unused on add
                    options.code.Value,
                    DateTime.UtcNow + options.from.Value,
                    DateTime.UtcNow + options.until.Value,
                    options.limit.Value,
                    used_count: 0,
                    options.reward.Value);

                var id = await couponRepository.AddCouponAsync(coupon);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"Successfully added coupon with id {id} 👍"
                ));
            },
            Preconditions: [
                new TaylorBotOwnerPrecondition()
            ]
        ));
    }
}
