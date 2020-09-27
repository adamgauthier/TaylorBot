import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { User } from 'discord.js';

export class UsernameRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async getHistory(user: User, limit: number): Promise<{ username: string; changed_at: Date }[]> {
        try {
            return await this.#db.any(
                `SELECT username, changed_at
                FROM users.usernames
                WHERE user_id = $[user_id]
                ORDER BY changed_at DESC
                LIMIT $[max_rows];`,
                {
                    user_id: user.id,
                    max_rows: limit
                }
            );
        }
        catch (e) {
            Log.error(`Getting username history for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async addNewUsernameAsync(user: User): Promise<void> {
        try {
            await this.#db.none(
                `INSERT INTO users.usernames (user_id, username) VALUES ($[user_id], $[username]);`,
                {
                    user_id: user.id,
                    username: user.username
                }
            );
        }
        catch (e) {
            Log.error(`Adding new username for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}
