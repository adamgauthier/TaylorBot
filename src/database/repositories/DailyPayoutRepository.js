'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class DailyPayoutRepository {
    constructor(db, usersDAO) {
        this._db = db;
        this._usersDAO = usersDAO;
    }

    async getCanRedeem(user) {
        try {
            return await this._db.oneOrNone(
                `SELECT last_payout_at < (CURRENT_TIMESTAMP at time zone 'EST')::date AS can_redeem,
                    ((CURRENT_TIMESTAMP + INTERVAL '1 DAY') at time zone 'EST')::date::timestamp with time zone AS can_redeem_at
                FROM users.daily_payouts WHERE user_id = $[user_id];`,
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting can redeem payout for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async giveDailyPay(user, payoutCount, streakForBonus, bonusCount) {
        try {
            return await this._db.tx(async t => {
                const { streak_count } = await this._db.one(
                    `INSERT INTO users.daily_payouts (user_id)
                    VALUES ($[user_id])
                    ON CONFLICT (user_id) DO UPDATE SET
                        last_payout_at = CURRENT_TIMESTAMP,
                        streak_count = (
                            CASE WHEN (daily_payouts.last_payout_at > ((CURRENT_TIMESTAMP - INTERVAL '1 DAY') at time zone 'EST')::date::timestamp with time zone)
                            THEN daily_payouts.streak_count + 1
                            ELSE 1
                        END)
                    RETURNING *;`,
                    {
                        user_id: user.id
                    }
                );

                const bonusPayoutCount =
                    global.BigInt(streak_count) % global.BigInt(streakForBonus) === global.BigInt(0) ? bonusCount : 0;

                const [{ taypoint_count }] = await this._usersDAO.addTaypointCount(t, [user], payoutCount + bonusPayoutCount);

                return {
                    taypoint_count,
                    streak_count,
                    payoutCount,
                    bonusPayoutCount
                };
            });
        }
        catch (e) {
            Log.error(`Giving daily payout ${payoutCount} to ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = DailyPayoutRepository;