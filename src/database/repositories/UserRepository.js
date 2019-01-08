'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class UserRepository {
    constructor(db) {
        this._db = db;
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
            return await this._db.oneOrNone(
                'SELECT taypoint_count FROM users.users WHERE user_id = $[user_id];',
                databaseUser
            );
        }
        catch (e) {
            Log.error(`Getting taypoint count for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async transferTaypointCount(userFrom, usersTo, amount) {
        try {
            return await this._db.tx(async t => {
                const { taypoint_count: originalCount } = await t.one(
                    'SELECT taypoint_count FROM users.users WHERE user_id = $[gifter_id];',
                    {
                        gifter_id: userFrom.id
                    }
                );

                const { taypoint_count: resultingCount } = await (amount.isRelative ?
                    t.one(
                        'UPDATE users.users SET taypoint_count = GREATEST(0, taypoint_count - FLOOR(taypoint_count / $[points_divisor])::bigint) WHERE user_id = $[gifter_id] RETURNING taypoint_count;',
                        {
                            points_divisor: amount.divisor,
                            gifter_id: userFrom.id
                        }
                    ) :
                    t.one(
                        'UPDATE users.users SET taypoint_count = GREATEST(0, taypoint_count - $[points_to_gift]) WHERE user_id = $[gifter_id] RETURNING taypoint_count;',
                        {
                            points_to_gift: amount.count,
                            gifter_id: userFrom.id
                        }
                    )
                );

                const totalGiftedCount = originalCount - resultingCount;

                const usersToGift = usersTo.map(user => ({
                    user,
                    giftedCount: Math.floor(totalGiftedCount / usersTo.length)
                }));
                usersToGift[0].giftedCount += totalGiftedCount % usersTo.length;

                for (const userTo of usersToGift.filter(({ giftedCount }) => giftedCount > 0)) {
                    await t.none(
                        'UPDATE users.users SET taypoint_count = taypoint_count + $[points_to_gift] WHERE user_id = $[receiver_id];',
                        {
                            points_to_gift: userTo.giftedCount,
                            receiver_id: userTo.user.id
                        }
                    );
                }

                return usersToGift;
            });
        }
        catch (e) {
            Log.error(`Transfering ${amount} taypoint count from ${Format.user(userFrom)} to ${usersTo.map(u => Format.user(u)).join()}: ${e}`);
            throw e;
        }
    }
}

module.exports = UserRepository;