'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class RpsStatsRepository {
    constructor(db, usersDAO) {
        this._db = db;
        this._usersDAO = usersDAO;
    }

    async winRpsGame(winnerUser, payoutCount) {
        try {
            return await this._db.tx(async t => {
                const [result] = await this._usersDAO.addTaypointCount(t, [winnerUser], payoutCount);

                await t.none(
                    `INSERT INTO users.rps_stats (user_id, rps_wins)
                    VALUES ($[user_id], $[win_count])
                    ON CONFLICT (user_id) DO UPDATE
                    SET rps_wins = rps_stats.rps_wins + $[win_count];`,
                    {
                        user_id: winnerUser.id,
                        win_count: 1
                    }
                );

                return result;
            });
        }
        catch (e) {
            Log.error(`Winning rps game with payout ${payoutCount} taypoint count for ${Format.user(winnerUser)}: ${e}`);
            throw e;
        }
    }
}

module.exports = RpsStatsRepository;