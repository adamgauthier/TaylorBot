using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Infrastructure
{
    public class DailyPayoutPostgresRepository : IDailyPayoutRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;
        private readonly IOptionsMonitor<DailyPayoutOptions> _options;

        public DailyPayoutPostgresRepository(PostgresConnectionFactory postgresConnectionFactory, IOptionsMonitor<DailyPayoutOptions> options)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
            _options = options;
        }

        private class CanRedeemDto
        {
            public bool can_redeem { get; set; }
            public DateTimeOffset can_redeem_at { get; set; }
        }

        public async ValueTask<ICanUserRedeemResult> CanUserRedeemAsync(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var canRedeem = await connection.QuerySingleOrDefaultAsync<CanRedeemDto>(
                @"SELECT
                    last_payout_at < date_trunc('day', CURRENT_TIMESTAMP) AS can_redeem,
                    date_trunc('day', ((CURRENT_TIMESTAMP + INTERVAL '1 DAY'))) AS can_redeem_at
                FROM users.daily_payouts
                WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString()
                }
            );

            return canRedeem == null || canRedeem.can_redeem ?
                new UserCanRedeem() :
                new UserCantRedeem(canRedeem.can_redeem_at);
        }

        private class RedeemDto
        {
            public bool was_streak_added { get; set; }
            public long streak_count { get; set; }
            public long bonus_reward { get; set; }
        }

        private class TaypointAddDto
        {
            public long taypoint_count { get; set; }
        }

        public async ValueTask<RedeemResult?> RedeemDailyPayoutAsync(IUser user)
        {
            var options = _options.CurrentValue;

            using var connection = _postgresConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            var redeem = await connection.QuerySingleAsync<RedeemDto>(
                @"INSERT INTO users.daily_payouts (user_id)
                VALUES (@UserId)
                ON CONFLICT (user_id) DO UPDATE SET
                    last_payout_at = (CASE
                        WHEN daily_payouts.last_payout_at < date_trunc('day', CURRENT_TIMESTAMP)
                        THEN CURRENT_TIMESTAMP
                        ELSE daily_payouts.last_payout_at
                    END),
                    streak_count = (CASE
                        WHEN daily_payouts.last_payout_at < date_trunc('day', CURRENT_TIMESTAMP)
                        THEN (CASE
                            WHEN (daily_payouts.last_payout_at > date_trunc('day', (CURRENT_TIMESTAMP - INTERVAL '1 DAY')))
                            THEN daily_payouts.streak_count + 1
                            ELSE 1
                        END)
                        ELSE daily_payouts.streak_count
                    END),
                    max_streak_count = GREATEST(daily_payouts.streak_count, daily_payouts.max_streak_count)
                RETURNING CURRENT_TIMESTAMP = last_payout_at AS was_streak_added, streak_count, CASE
                    WHEN streak_count % @DaysForBonus = 0
                    THEN (@BaseBonus + @BonusMultiplier * SQRT(streak_count))::bigint
                    ELSE 0
                END AS bonus_reward;",
                new
                {
                    UserId = user.Id.ToString(),
                    DaysForBonus = (long)options.DaysForBonus,
                    BaseBonus = (long)options.BaseBonusAmount,
                    BonusMultiplier = (long)options.IncreasingBonusModifier
                }
            );

            if (redeem.was_streak_added)
            {
                var taypointAdd = await connection.QuerySingleAsync<TaypointAddDto>(
                    @"UPDATE users.users SET taypoint_count = taypoint_count + @PointsToAdd WHERE user_id = @UserId RETURNING taypoint_count;",
                    new
                    {
                        PointsToAdd = options.DailyPayoutAmount + redeem.bonus_reward,
                        UserId = user.Id.ToString()
                    }
                );
                transaction.Commit();

                return new RedeemResult(
                    PayoutAmount: options.DailyPayoutAmount,
                    BonusAmount: redeem.bonus_reward,
                    TotalTaypointCount: taypointAdd.taypoint_count,
                    CurrentDailyStreak: redeem.streak_count,
                    DaysForBonus: options.DaysForBonus
                );
            }
            else
            {
                transaction.Commit();
                return null;
            }
        }
    }
}

