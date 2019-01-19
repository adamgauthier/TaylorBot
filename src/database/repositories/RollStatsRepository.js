'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class RollStatsRepository {
    constructor(db, usersDAO) {
        this._db = db;
        this._usersDAO = usersDAO;
    }

    _addRollCount(queryable, user, rollCount) {
        return queryable.none(
            `INSERT INTO users.roll_stats (user_id, roll_count)
            VALUES ($[user_id], $[roll_count])
            ON CONFLICT (user_id) DO UPDATE SET
                roll_count = roll_stats.roll_count + $[roll_count]
            ;`,
            {
                user_id: user.id,
                roll_count: rollCount
            }
        );
    }

    async addRollCount(userTo, rollCount) {
        try {
            return await this._addRollCount(this._db, userTo, rollCount);
        }
        catch (e) {
            Log.error(`Adding ${rollCount} roll count to ${Format.user(userTo)}: ${e}`);
            throw e;
        }
    }

    async winRoll(winnerUser, payoutCount) {
        try {
            return await this._db.tx(async t => {
                const [result] = await this._usersDAO.addTaypointCount(t, [winnerUser], payoutCount);

                await this._addRollCount(t, winnerUser, 1);

                return result;
            });
        }
        catch (e) {
            Log.error(`Winning roll with payout ${payoutCount} taypoint count for ${Format.user(winnerUser)}: ${e}`);
            throw e;
        }
    }

    async winPerfectRoll(winnerUser, payoutCount) {
        try {
            return await this._db.tx(async t => {
                const [result] = await this._usersDAO.addTaypointCount(t, [winnerUser], payoutCount);

                await t.none(
                    `INSERT INTO users.roll_stats (user_id, roll_count, perfect_roll_count)
                    VALUES ($[user_id], $[win_count], $[win_count])
                    ON CONFLICT (user_id) DO UPDATE SET
                        roll_count = roll_stats.roll_count + $[win_count],
                        perfect_roll_count = roll_stats.perfect_roll_count + $[win_count]
                    ;`,
                    {
                        user_id: winnerUser.id,
                        win_count: 1
                    }
                );

                return result;
            });
        }
        catch (e) {
            Log.error(`Winning perfect roll with payout ${payoutCount} taypoint count for ${Format.user(winnerUser)}: ${e}`);
            throw e;
        }
    }
}

module.exports = RollStatsRepository;