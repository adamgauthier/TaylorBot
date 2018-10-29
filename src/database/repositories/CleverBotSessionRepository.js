'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class CleverBotSessionRepository {
    constructor(db) {
        this._db = db;
    }

    async get(user) {
        try {
            return await this._db.instance.oneOrNone(
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
                'INSERT INTO users.cleverbot_sessions (user_id, session_created_at) VALUES ($[user_id], $[session_created_at]);',
                {
                    'user_id': user.id,
                    'session_created_at': Date.now()
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