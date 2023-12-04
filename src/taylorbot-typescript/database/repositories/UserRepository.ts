import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { User } from 'discord.js';

export class UserRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    mapUserToDatabase(user: User): { user_id: string } {
        return {
            'user_id': user.id
        };
    }

    async hasEnoughTaypointCount(user: User, taypointCount: number): Promise<{ taypoint_count: string; has_enough: boolean }> {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this.#db.one(
                'SELECT taypoint_count, taypoint_count >= $[taypoint_count] AS has_enough FROM users.users WHERE user_id = $[user_id];',
                {
                    ...databaseUser,
                    taypoint_count: taypointCount
                }
            );
        }
        catch (e) {
            Log.error(`Getting has enough taypoint count ${taypointCount} for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async insertOrGetUserIgnoreUntil(user: User): Promise<{ ignore_until: Date; was_inserted: boolean; username_changed: boolean; previous_username: string | null }> {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this.#db.one(
                `INSERT INTO users.users (user_id, is_bot, username, previous_username) VALUES ($[user_id], $[is_bot], $[username], NULL)
                ON CONFLICT (user_id) DO UPDATE SET
                    previous_username = users.users.username,
                    username = excluded.username
                RETURNING
                    ignore_until, previous_username IS NULL AS was_inserted,
                    previous_username IS DISTINCT FROM username AS username_changed, previous_username;`,
                { ...databaseUser, is_bot: !!user.bot, username: user.username }
            );
        }
        catch (e) {
            Log.error(`Inserting or getting user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async ignore(user: User, ignoreUntil: Date): Promise<void> {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            await this.#db.none(
                'UPDATE users.users SET ignore_until = $[ignore_until] WHERE user_id = $[user_id];',
                { ...databaseUser, ignore_until: ignoreUntil }
            );
        }
        catch (e) {
            Log.error(`Ignoring user ${Format.user(user)} until ${ignoreUntil}: ${e}`);
            throw e;
        }
    }
}
