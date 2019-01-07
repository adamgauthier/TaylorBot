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
}

module.exports = UserRepository;