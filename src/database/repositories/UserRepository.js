'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class UserRepository {
    constructor(db, usersDAO) {
        this._db = db;
        this._usersDAO = usersDAO;
    }

    async getAll() {
        try {
            return await this._db.any('SELECT user_id, ignore_until FROM users.users;');
        }
        catch (e) {
            Log.error(`Getting all users: ${e}`);
            throw e;
        }
    }

    mapUserToDatabase(user) {
        return {
            'user_id': user.id
        };
    }

    async get(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.oneOrNone(
                'SELECT * FROM users.users WHERE user_id = $[user_id];',
                databaseUser
            );
        }
        catch (e) {
            Log.error(`Getting user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async add(member, discoveredAt) {
        const { user, guild, joinedTimestamp } = member;

        try {
            return await this._db.tx(async t => {
                const inserted = await t.one(
                    'INSERT INTO users.users (user_id) VALUES ($1) RETURNING *;',
                    [user.id]
                );
                await t.none(
                    'INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES ($1, $2, $3);',
                    [guild.id, user.id, joinedTimestamp]
                );
                await t.none(
                    'INSERT INTO users.usernames (user_id, username, changed_at) VALUES ($1, $2, $3);',
                    [user.id, user.username, discoveredAt]
                );

                return inserted;
            });
        }
        catch (e) {
            Log.error(`Adding new user from member ${Format.member(member)}: ${e}`);
            throw e;
        }
    }

    async getTaypointCount(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.one(
                'SELECT taypoint_count FROM users.users WHERE user_id = $[user_id];',
                databaseUser
            );
        }
        catch (e) {
            Log.error(`Getting taypoint count for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async hasEnoughTaypointCount(user, taypointCount) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.one(
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

    async transferTaypointCount(userFrom, usersTo, amount) {
        try {
            return await this._db.tx(async t => {
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

    async addTaypointCount(usersTo, count) {
        try {
            return await this._usersDAO.addTaypointCount(this._db, usersTo, count);
        }
        catch (e) {
            Log.error(`Adding ${count} taypoint count to ${usersTo.map(u => Format.user(u)).join()}: ${e}`);
            throw e;
        }
    }

    async ignore(user, ignoreUntil) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.one(
                'UPDATE users.users SET ignore_until = $[ignore_until] WHERE user_id = $[user_id] RETURNING *;',
                { ...databaseUser, ignore_until: ignoreUntil }
            );
        }
        catch (e) {
            Log.error(`Ignoring user ${Format.user(user)} until ${ignoreUntil}: ${e}`);
            throw e;
        }
    }
}

module.exports = UserRepository;