'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class DailyPayoutRepository {
    constructor(db, usersDAO) {
        this._db = db;
        this._usersDAO = usersDAO;
    }

    async isLastPayoutInLast24Hours(user) {
        try {
            return await this._db.oneOrNone(
                `SELECT (last_payout_at + INTERVAL '1 DAY') AS can_redeem_at
                FROM users.daily_payouts WHERE user_id = $[user_id];`,
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Checking if last payout was in the last 24 hours for ${Format.user(user)}: ${e}`);
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