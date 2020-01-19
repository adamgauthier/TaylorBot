'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class UsernameRepository {
    constructor(db) {
        this._db = db;
    }

    mapUserToUsernameDatabase(user) {
        return {
            user_id: user.id,
            username: user.username
        };
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

    async addNewUsernameAsync(user) {
        try {
            return await this._db.none(
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

module.exports = UsernameRepository;
