using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using OperationResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Infrastructure;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Infrastructure;

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
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var canRedeem = await connection.QuerySingleOrDefaultAsync<CanRedeemDto>(
            """
            SELECT
                last_payout_at < date_trunc('day', CURRENT_TIMESTAMP) AS can_redeem,
                date_trunc('day', ((CURRENT_TIMESTAMP + INTERVAL '1 DAY'))) AS can_redeem_at
            FROM users.daily_payouts
            WHERE user_id = @UserId;
            """,
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

    public async ValueTask<RedeemResult?> RedeemDailyPayoutAsync(IUser user, uint payoutAmount)
    {
        var options = _options.CurrentValue;

        await using var connection = _postgresConnectionFactory.CreateConnection();
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
                    PointsToAdd = payoutAmount + redeem.bonus_reward,
                    UserId = user.Id.ToString()
                }
            );
            transaction.Commit();

            return new RedeemResult(
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

    private class StreakInfoDto
    {
        public long streak_count { get; set; }
        public long max_streak_count { get; set; }
    }

    public async ValueTask<(long CurrentStreak, long MaxStreak)?> GetStreakInfoAsync(IUser user)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var streakInfo = await connection.QuerySingleOrDefaultAsync<StreakInfoDto?>(
            @"SELECT streak_count, max_streak_count
            FROM users.daily_payouts
            WHERE user_id = @UserId;",
            new
            {
                UserId = user.Id.ToString()
            }
        );

        return streakInfo != null ? (streakInfo.streak_count, streakInfo.max_streak_count) : null;
    }

    private class UpdateStreakDto
    {
        public long streak_count { get; set; }
        public long original_streak_count { get; set; }
    }

    private class RemoveTaypointCountDto
    {
        public long original_count { get; set; }
        public long taypoint_count { get; set; }
        public long lost_count { get; set; }
    }

    public async ValueTask<Result<RebuyResult, RebuyFailed>> RebuyMaxStreakAsync(IUser user, int pricePerDay)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var updateStreak = await connection.QuerySingleAsync<UpdateStreakDto>(
            @"WITH old_daily AS (
                SELECT user_id, streak_count FROM users.daily_payouts
                WHERE user_id = @UserId FOR UPDATE
            )
            UPDATE users.daily_payouts AS d
            SET streak_count = GREATEST(d.streak_count, d.max_streak_count), last_payout_at = CURRENT_TIMESTAMP
            FROM old_daily
            WHERE d.user_id = old_daily.user_id
            RETURNING d.streak_count, old_daily.streak_count AS original_streak_count;",
            new
            {
                UserId = user.Id.ToString()
            }
        );

        if (updateStreak.streak_count > updateStreak.original_streak_count)
        {
            var cost = updateStreak.streak_count * pricePerDay;

            var removeTaypointCount = await connection.QuerySingleAsync<RemoveTaypointCountDto>(
                @"UPDATE users.users AS u
                SET taypoint_count = GREATEST(0, taypoint_count - @PointsToRemove)
                FROM (
                    SELECT user_id, taypoint_count AS original_count
                    FROM users.users WHERE user_id = @UserId FOR UPDATE
                ) AS old_u
                WHERE u.user_id = old_u.user_id
                RETURNING old_u.original_count, u.taypoint_count, (old_u.original_count - u.taypoint_count) AS lost_count;",
                new
                {
                    PointsToRemove = cost,
                    UserId = user.Id.ToString()
                }
            );

            if (removeTaypointCount.lost_count < cost)
            {
                transaction.Rollback();
                return Error(new RebuyFailed(removeTaypointCount.original_count));
            }
            else
            {
                transaction.Commit();
                return new RebuyResult(removeTaypointCount.taypoint_count, updateStreak.streak_count);
            }
        }
        else
        {
            transaction.Rollback();
            throw new InvalidOperationException();
        }
    }

    private class LeaderboardEntryDto
    {
        public string user_id { get; set; } = null!;
        public string username { get; set; } = null!;
        public long streak_count { get; set; }
        public long rank { get; set; }
    }

    public async ValueTask<IList<DailyLeaderboardEntry>> GetLeaderboardAsync()
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var entries = await connection.QueryAsync<LeaderboardEntryDto>(
            @"SELECT u.user_id, u.username, dp.streak_count, rank() OVER (ORDER BY streak_count DESC) AS rank
            FROM users.daily_payouts AS dp INNER JOIN users.users AS u ON dp.user_id = u.user_id
            LIMIT 100;"
        );

        return entries.Select(e => new DailyLeaderboardEntry(
            new(e.user_id),
            e.username,
            e.streak_count,
            e.rank
        )).ToList();
    }
}
