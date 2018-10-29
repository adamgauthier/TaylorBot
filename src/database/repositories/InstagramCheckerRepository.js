'use strict';

const Log = require('../../tools/Logger.js');

class InstagramCheckerRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.any('SELECT * FROM checkers.instagram_checker;');
        }
        catch (e) {
            Log.error(`Getting Instagrams: ${e}`);
            throw e;
        }
    }

    async update(instagramUsername, guildId, channelId, lastCode) {
        try {
            return await this._db.oneOrNone(
                `UPDATE checkers.instagram_checker SET last_post_code $[last_post_code]
                WHERE instagram_username = $[instagram_username] AND guild_id = $[guild_id] AND channel_id = $[channel_id]
                RETURNING *;`,
                {
                    'last_post_code': lastCode,
                    'instagram_username': instagramUsername,
                    'guild_id': guildId,
                    'channel_id': channelId
                }
            );
        }
        catch (e) {
            Log.error(`Updating Instagram for guild ${guildId}, channel ${channelId}, username ${instagramUsername}: ${e}`);
            throw e;
        }
    }
}

module.exports = InstagramCheckerRepository;