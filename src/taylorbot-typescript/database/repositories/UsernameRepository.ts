import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { User } from 'discord.js';

export class UsernameRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
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
