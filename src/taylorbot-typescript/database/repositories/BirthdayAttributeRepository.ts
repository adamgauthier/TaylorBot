import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import pgPromise = require('pg-promise');
import { User } from 'discord.js';

export class BirthdayAttributeRepository {
    #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async get(user: User): Promise<{ birthday: string; is_private: boolean } | null> {
        try {
            return await this.#db.oneOrNone(
                `SELECT birthday::text, is_private FROM attributes.birthdays WHERE user_id = $[user_id];`,
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting birthday attribute for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}
