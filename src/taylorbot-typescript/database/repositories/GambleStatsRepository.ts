import Log = require('../../tools/Logger.js');
import Format = require('../../modules/DiscordFormatter.js');
import * as pgPromise from 'pg-promise';
import { UserDAO } from '../dao/UserDAO';
import { User } from 'discord.js';
import { TaypointAmount } from '../../modules/points/TaypointAmount';

export class GambleStatsRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    readonly #usersDAO: UserDAO;

    constructor(db: pgPromise.IDatabase<unknown>, usersDAO: UserDAO) {
        this.#db = db;
        this.#usersDAO = usersDAO;
    }

    async loseGambledTaypointCount(userTo: User, amount: TaypointAmount): Promise<{ gambled_count: string; original_count: string; final_count: string }> {
        try {
            return await this.#db.tx(async t => {
                const result = await this.#usersDAO.loseBet(t, userTo.id, amount);

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

    async winGambledTaypointCount(userTo: User, amount: TaypointAmount, payoutMultiplier: string): Promise<{ gambled_count: string; original_count: string; final_count: string }> {
        try {
            return await this.#db.tx(async t => {
                const result = await this.#usersDAO.winBet(t, userTo.id, amount, payoutMultiplier);

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
