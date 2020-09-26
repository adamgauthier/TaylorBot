import Log = require('../../tools/Logger.js');
import Format = require('../../modules/DiscordFormatter.js');
import * as pgPromise from 'pg-promise';
import { UserDAO } from '../dao/UserDAO';
import { User } from 'discord.js';

export class RollStatsRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    readonly #usersDAO: UserDAO;

    constructor(db: pgPromise.IDatabase<unknown>, usersDAO: UserDAO) {
        this.#db = db;
        this.#usersDAO = usersDAO;
    }

    async _addRollCount(queryable: pgPromise.IBaseProtocol<unknown>, user: User, rollCount: number): Promise<void> {
        await queryable.none(
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

    async addRollCount(userTo: User, rollCount: number): Promise<void> {
        try {
            await this._addRollCount(this.#db, userTo, rollCount);
        }
        catch (e) {
            Log.error(`Adding ${rollCount} roll count to ${Format.user(userTo)}: ${e}`);
            throw e;
        }
    }

    async winRoll(winnerUser: User, payoutCount: number): Promise<void> {
        try {
            await this.#db.tx(async t => {
                await this._addRollCount(t, winnerUser, 1);

                const [result] = await this.#usersDAO.addTaypointCount(t, [winnerUser], payoutCount);

                return result;
            });
        }
        catch (e) {
            Log.error(`Winning roll with payout ${payoutCount} taypoint count for ${Format.user(winnerUser)}: ${e}`);
            throw e;
        }
    }

    async winPerfectRoll(winnerUser: User, payoutCount: number): Promise<void> {
        try {
            await this.#db.tx(async t => {
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

                const [result] = await this.#usersDAO.addTaypointCount(t, [winnerUser], payoutCount);

                return result;
            });
        }
        catch (e) {
            Log.error(`Winning perfect roll with payout ${payoutCount} taypoint count for ${Format.user(winnerUser)}: ${e}`);
            throw e;
        }
    }
}
