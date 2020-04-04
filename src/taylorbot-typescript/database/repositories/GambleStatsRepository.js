'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GambleStatsRepository {
    constructor(db, usersDAO) {
        this._db = db;
        this._usersDAO = usersDAO;
    }

    async loseGambledTaypointCount(userTo, amount) {
        try {
            return await this._db.tx(async t => {
                const result = await this._usersDAO.loseBet(t, userTo.id, amount);

                await t.none(
                    `INSERT INTO users.gamble_stats (user_id, gamble_lose_count, gamble_lose_amount)
                    VALUES ($[user_id], $[lose_count], $[lose_amount])
                    ON CONFLICT (user_id) DO UPDATE SET
                        gamble_lose_count = gamble_stats.gamble_lose_count + $[lose_count],
                        gamble_lose_amount = gamble_stats.gamble_lose_amount + $[lose_amount]
                    ;`,
                    {
                        user_id: userTo.id,
                        lose_count: 1,
                        lose_amount: result.lost_count
                    }
                );

                return result;
            });
        }
        catch (e) {
            Log.error(`Losing ${amount} taypoint amount for ${Format.user(userTo)}: ${e}`);
            throw e;
        }
    }

    async winGambledTaypointCount(userTo, amount, payoutMultiplier) {
        try {
            return await this._db.tx(async t => {
                const result = await this._usersDAO.winBet(t, userTo.id, amount, payoutMultiplier);

                await t.none(
                    `INSERT INTO users.gamble_stats (user_id, gamble_win_count, gamble_win_amount)
                    VALUES ($[user_id], $[win_count], $[win_amount])
                    ON CONFLICT (user_id) DO UPDATE SET
                        gamble_win_count = gamble_stats.gamble_win_count + $[win_count],
                        gamble_win_amount = gamble_stats.gamble_win_amount + $[win_amount]
                    ;`,
                    {
                        user_id: userTo.id,
                        win_count: 1,
                        win_amount: result.payout_count
                    }
                );

                return result;
            });
        }
        catch (e) {
            Log.error(`Winning ${amount} taypoint amount for ${Format.user(userTo)}: ${e}`);
            throw e;
        }
    }
}

module.exports = GambleStatsRepository;