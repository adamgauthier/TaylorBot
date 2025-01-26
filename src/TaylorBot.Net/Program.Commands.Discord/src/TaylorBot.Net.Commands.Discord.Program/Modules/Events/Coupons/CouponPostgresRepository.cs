using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Taypoints;
using TaylorBot.Net.Core.User;
using static Dapper.SqlMapper;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Coupons;

public class CouponPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ICouponRepository
{
    public async Task<Coupon?> GetCouponAsync(string code)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Coupon>(
            """
            SELECT coupon_id, code, valid_from, valid_until, usage_limit, used_count, taypoint_reward
            FROM commands.coupons
            WHERE code = @Code;
            """,
            new
            {
                Code = code,
            }
        );
    }

    public async Task<IList<RedeemedCoupon>> GetRedeemedCouponsAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return (await connection.QueryAsync<RedeemedCoupon>(
            """
            SELECT redeemed_at, coupon_code, coupon_reward
            FROM users.redeemed_coupons
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        )).ToList();
    }

    public async Task<TaypointAddResult?> RedeemCouponAsync(DiscordUser user, Coupon coupon)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = await connection.BeginTransactionAsync();

        var added = await connection.QuerySingleOrDefaultAsync<bool>(
            """
            INSERT INTO users.redeemed_coupons (user_id, coupon_id, coupon_code, coupon_reward)
            VALUES (@UserId, @CouponId, @CouponCode, @CouponReward)
            ON CONFLICT (user_id, coupon_id) DO NOTHING
            RETURNING TRUE;
            """,
            new
            {
                UserId = $"{user.Id}",
                CouponId = coupon.coupon_id,
                CouponCode = coupon.code,
                CouponReward = coupon.taypoint_reward,
            }
        );

        if (!added)
        {
            await transaction.RollbackAsync();
            return null;
        }

        await connection.ExecuteAsync(
            """
            UPDATE commands.coupons
            SET used_count = used_count + 1
            WHERE coupon_id = @CouponId;
            """,
            new
            {
                CouponId = coupon.coupon_id,
            }
        );

        var taypointAddResult = await TaypointPostgresUtil.AddTaypointsReturningAsync(connection, user.Id, coupon.taypoint_reward);
        await transaction.CommitAsync();

        return taypointAddResult;
    }

    public async Task<Guid> AddCouponAsync(Coupon coupon)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<Guid>(
            """
            INSERT INTO commands.coupons (code, valid_from, valid_until, usage_limit, taypoint_reward)
            VALUES (@Code, @ValidFrom, @ValidUntil, @UsageLimit, @TaypointReward)
            RETURNING coupon_id;
            """,
            new
            {
                Code = coupon.code,
                ValidFrom = coupon.valid_from,
                ValidUntil = coupon.valid_until,
                UsageLimit = coupon.usage_limit,
                TaypointReward = coupon.taypoint_reward,
            }
        );
    }
}
