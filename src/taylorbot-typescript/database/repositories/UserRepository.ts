import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { User } from 'discord.js';
import { TaypointAmount } from '../../modules/points/TaypointAmount';

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

    async getTaypointsFor(user: User): Promise<{ taypoint_count: string; }> {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this.#db.one(
                'SELECT taypoint_count FROM users.users WHERE user_id = $[user_id];',
                {
                    ...databaseUser
                }
            );
        }
        catch (e) {
            Log.error(`Getting taypoints for user ${Format.user(user)}: ${e}`);
            throw e;
        }
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

    async transferTaypointCount(userFrom: User, usersTo: User[], amount: TaypointAmount): Promise<{
        usersToGift: { user: User; giftedCount: number }[];
        gifted_count: string;
        original_count: string;
    }> {
        try {
            return await this.#db.tx(async t => {
                const toRemove = amount.isRelative ?
                    { query: 'FLOOR(taypoint_count / $[points_divisor])::bigint', params: { points_divisor: amount.divisor } } :
                    { query: '$[points_to_gift]', params: { points_to_gift: amount.count } };

                const { original_count, gifted_count } = await t.one(
                    `UPDATE users.users AS u
                    SET taypoint_count = GREATEST(0, taypoint_count - ${toRemove.query})
                    FROM (
                        SELECT user_id, taypoint_count AS original_count
                        FROM users.users WHERE user_id = $[gifter_id] FOR UPDATE
                    ) AS old_u
                    WHERE u.user_id = old_u.user_id
                    RETURNING old_u.original_count, (old_u.original_count - u.taypoint_count) AS gifted_count;`,
                    {
                        ...toRemove.params,
                        gifter_id: userFrom.id
                    }
                );

                const baseGiftCount = Math.floor(gifted_count / usersTo.length);
                const [firstUser, ...others] = usersTo;
                const usersToGift = [
                    { user: firstUser, giftedCount: baseGiftCount + gifted_count % usersTo.length },
                    ...others.map(user => ({ user, giftedCount: baseGiftCount }))
                ];

                for (const userTo of usersToGift.filter(({ giftedCount }) => giftedCount > 0)) {
                    await t.none(
                        'UPDATE users.users SET taypoint_count = taypoint_count + $[points_to_gift] WHERE user_id = $[receiver_id];',
                        {
                            points_to_gift: userTo.giftedCount,
                            receiver_id: userTo.user.id
                        }
                    );
                }

                return { gifted_count, usersToGift, original_count };
            });
        }
        catch (e) {
            Log.error(`Transferring ${amount} taypoint amount from ${Format.user(userFrom)} to ${usersTo.map(u => Format.user(u)).join()}: ${e}`);
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
