import Log = require('../../tools/Logger.js');
import Format = require('../../modules/DiscordFormatter.js');
import * as pgPromise from 'pg-promise';
import { UserDAO } from '../dao/UserDAO';
import { User } from 'discord.js';

export class RpsStatsRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    readonly #usersDAO: UserDAO;

    constructor(db: pgPromise.IDatabase<unknown>, usersDAO: UserDAO) {
        this.#db = db;
        this.#usersDAO = usersDAO;
    }

    async winRpsGame(winnerUser: User, payoutCount: number): Promise<{ taypoint_count: string }> {
        try {
            return await this.#db.tx(async t => {
                await t.none(
                    `INSERT INTO users.rps_stats (user_id, rps_win_count)
                    VALUES ($[user_id], $[win_count])
                    ON CONFLICT (user_id) DO UPDATE
                    SET rps_win_count = rps_stats.rps_win_count + $[win_count];`,
                    {
                        user_id: winnerUser.id,
                        win_count: 1
                    }
                );

                const [result] = await this.#usersDAO.addTaypointCount(t, [winnerUser], payoutCount);

                return result;
            });
        }
        catch (e) {
            Log.error(`Winning rps game with payout ${payoutCount} taypoint count for ${Format.user(winnerUser)}: ${e}`);
            throw e;
        }
    }
}
