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

    async giveDailyPay(user, payoutCount) {
        try {
            return await this._db.tx(async t => {
                await this._db.none(
                    `INSERT INTO users.daily_payouts (user_id)
                    VALUES ($[user_id])
                    ON CONFLICT (user_id) DO UPDATE SET
                        last_payout_at = NOW()
                    ;`,
                    {
                        user_id: user.id
                    }
                );

                const [result] = await this._usersDAO.addTaypointCount(t, [user], payoutCount);

                return result;
            });
        }
        catch (e) {
            Log.error(`Giving daily payout ${payoutCount} to ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = DailyPayoutRepository;