'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class UsernameRepository {
    constructor(db) {
        this._db = db;
    }

    async getLatest(user) {
        try {
            return await this._db.oneOrNone(
                `SELECT u.username, u.user_id
                FROM (
                    SELECT user_id, MAX(changed_at) AS max_changed_at
                    FROM users.usernames
                    GROUP BY user_id
                ) AS maxed
                JOIN users.usernames AS u ON u.user_id = maxed.user_id AND u.changed_at = maxed.max_changed_at
                WHERE u.user_id = $[user_id];`,
                {
                    'user_id': user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting latest username for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    mapUserToUsernameDatabase(user) {
        return {
            user_id: user.id,
            username: user.username
        };
    }

    async add(user) {
        const databaseUsername = this.mapUserToUsernameDatabase(user);
        try {
            return await this._db.none(
                'INSERT INTO users.usernames (user_id, username) VALUES ($[user_id], $[username]);',
                databaseUsername
            );
        }
        catch (e) {
            Log.error(`Adding username for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async getHistory(user, limit) {
        try {
            return await this._db.any(
                `SELECT username, changed_at
                FROM users.usernames
                WHERE user_id = $[user_id]
                ORDER BY changed_at DESC
                LIMIT $[max_rows];`,
                {
                    'user_id': user.id,
                    'max_rows': limit
                }
            );
        }
        catch (e) {
            Log.error(`Getting username history for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = UsernameRepository;