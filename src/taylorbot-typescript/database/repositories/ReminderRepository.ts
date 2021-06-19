import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { User } from 'discord.js';

export class ReminderRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async removeFrom(user: User): Promise<any[]> {
        try {
            return await this.#db.any(
                'DELETE FROM users.reminders WHERE user_id = $[user_id] RETURNING reminder_id;',
                { user_id: user.id }
            );
        }
        catch (e) {
            Log.error(`Removing reminders for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}
