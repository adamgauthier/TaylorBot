'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GambleStatsRepository {
    constructor(db) {
        this._db = db;
    }

    async loseGambledTaypointCount(userTo, amount) {
        try {
            return await this._db.tx(async t => {
                const toRemove = amount.isRelative ?
                    { query: 'FLOOR(taypoint_count / $[points_divisor])::bigint', params: { points_divisor: amount.divisor } } :
                    { query: 'LEAST(taypoint_count, $[gamble_points])', params: { gamble_points: amount.count } };

                const result = await t.one(
                    `UPDATE users.users AS u
                    SET taypoint_count = GREATEST(0, taypoint_count - ${toRemove.query})
                    FROM (
                        SELECT user_id, ${toRemove.query} AS gambled_count, taypoint_count AS original_count
                        FROM users.users WHERE user_id = $[user_id] FOR UPDATE
                    ) AS old_u
                    WHERE u.user_id = old_u.user_id
                    RETURNING old_u.gambled_count, old_u.original_count, u.taypoint_count AS final_count, old_u.original_count - u.taypoint_count AS lost_count;`,
                    {
                        ...toRemove.params,
                        user_id: userTo.id
                    }
                );

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
                const toAdd = amount.isRelative ?
                    { query: 'FLOOR(taypoint_count / $[points_divisor])::bigint', params: { points_divisor: amount.divisor } } :
                    { query: 'LEAST(taypoint_count, $[gamble_points])', params: { gamble_points: amount.count } };

                const result = await t.one(
                    `UPDATE users.users AS u
                    SET taypoint_count = taypoint_count + (${toAdd.query} * $[payout_multiplier])
                    FROM (
                        SELECT user_id, ${toAdd.query} AS gambled_count, taypoint_count AS original_count
                        FROM users.users WHERE user_id = $[user_id] FOR UPDATE
                    ) AS old_u
                    WHERE u.user_id = old_u.user_id
                    RETURNING old_u.gambled_count, old_u.original_count, u.taypoint_count AS final_count, u.taypoint_count - old_u.original_count AS payout_count;`,
                    {
                        ...toAdd.params,
                        payout_multiplier: payoutMultiplier,
                        user_id: userTo.id
                    }
                );

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