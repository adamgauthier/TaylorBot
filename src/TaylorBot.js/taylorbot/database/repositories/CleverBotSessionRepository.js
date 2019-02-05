'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class CleverBotSessionRepository {
    constructor(db) {
        this._db = db;
    }

    async getRandom() {
        try {
            return await this._db.oneOrNone(
                'SELECT * FROM users.cleverbot_sessions ORDER BY random() LIMIT 1;'
            );
        }
        catch (e) {
            Log.error(`Getting random cleverbot session: ${e}`);
            throw e;
        }
    }

    async get(user) {
        try {
            return await this._db.oneOrNone(
                'SELECT * FROM users.cleverbot_sessions WHERE user_id = $[user_id];',
                {
                    'user_id': user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting cleverbot session for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async add(user) {
        try {
            return await this._db.none(
                'INSERT INTO users.cleverbot_sessions (user_id) VALUES ($[user_id]);',
                {
                    'user_id': user.id
                }
            );
        }
        catch (e) {
            Log.error(`Adding cleverbot session for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = CleverBotSessionRepository;