import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { User } from 'discord.js';

export class ReminderRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async fromUser(user: User): Promise<any[]> {
        try {
            return await this.#db.any(
                'SELECT reminder_id FROM users.reminders WHERE user_id = $[user_id];',
                {
                    'user_id': user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting reminder from user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async add(user: User, remindAt: Date, reminderText: string): Promise<void> {
        try {
            await this.#db.none(
                `INSERT INTO users.reminders (user_id, remind_at, reminder_text)
                VALUES ($[user_id], $[remind_at], $[reminder_text]);`,
                {
                    'user_id': user.id,
                    'remind_at': remindAt,
                    'reminder_text': reminderText
                }
            );
        }
        catch (e) {
            Log.error(`Adding reminder for user ${Format.user(user)}: ${e}`);
            throw e;
        }
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
