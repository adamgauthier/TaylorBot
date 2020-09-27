import { Log } from '../../tools/Logger';
import * as pgPromise from 'pg-promise';
import { TaypointAmount } from '../../modules/points/TaypointAmount';
import { UserDAO } from '../dao/UserDAO';

export class HeistStatsRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    readonly #usersDAO: UserDAO;

    constructor(db: pgPromise.IDatabase<unknown>, usersDAO: UserDAO) {
        this.#db = db;
        this.#usersDAO = usersDAO;
    }

    async loseHeist(heisters: { userId: string; amount: TaypointAmount }[]): Promise<{ user_id: string; gambled_count: string; final_count: string; lost_count: string }[]> {
        try {
            return await this.#db.tx(async t => {
                const results = [];

                for (const { userId, amount } of heisters) {
                    const result = await this.#usersDAO.loseBet(t, userId, amount);

                    await t.none(
                        `INSERT INTO users.heist_stats (user_id, heist_lose_count, heist_lose_amount)
                        VALUES ($[user_id], $[lose_count], $[lose_amount])
                        ON CONFLICT (user_id) DO UPDATE SET
                            heist_lose_count = heist_stats.heist_lose_count + $[lose_count],
                            heist_lose_amount = heist_stats.heist_lose_amount + $[lose_amount]
                        ;`,
                        {
                            user_id: userId,
                            lose_count: 1,
                            lose_amount: result.lost_count
                        }
                    );

                    results.push(result);
                }

                return results;
            });
        }
        catch (e) {
            Log.error(`Losing heist for ${heisters.map(({ userId, amount }) => `${userId}-${amount}`).join()}: ${e}`);
            throw e;
        }
    }

    async winHeist(heisters: { userId: string; amount: TaypointAmount }[], payoutMultiplier: string): Promise<{ user_id: string; gambled_count: string; final_count: string; payout_count: string }[]> {
        try {
            return await this.#db.tx(async t => {
                const results = [];

                for (const { userId, amount } of heisters) {
                    const result = await this.#usersDAO.winBet(t, userId, amount, payoutMultiplier);

                    await t.none(
                        `INSERT INTO users.heist_stats (user_id, heist_win_count, heist_win_amount)
                        VALUES ($[user_id], $[win_count], $[win_amount])
                        ON CONFLICT (user_id) DO UPDATE SET
                            heist_win_count = heist_stats.heist_win_count + $[win_count],
                            heist_win_amount = heist_stats.heist_win_amount + $[win_amount]
                        ;`,
                        {
                            user_id: userId,
                            win_count: 1,
                            win_amount: result.payout_count
                        }
                    );

                    results.push(result);
                }

                return results;
            });
        }
        catch (e) {
            Log.error(`Winning heist for ${heisters.map(({ userId, amount }) => `${userId}-${amount}`).join()}: ${e}`);
            throw e;
        }
    }
}
