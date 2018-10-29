'use strict';

const Log = require('../../tools/Logger.js');

class TumblrCheckerRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.instance.any('SELECT * FROM checkers.tumblr_checker;');
        }
        catch (e) {
            Log.error(`Getting Tumblrs: ${e}`);
            throw e;
        }
    }

    async update(tumblrUser, guildId, channelId, lastLink) {
        try {
            return await this._db.instance.oneOrNone(
                `UPDATE checkers.tumblr_checker SET last_link = $[last_link]
                WHERE tumblr_user = $[tumblr_user] AND guild_id = $[guild_id] AND channel_id = $[channel_id]
                RETURNING *;`,
                {
                    'last_link': lastLink,
                    'tumblr_user': tumblrUser,
                    'guild_id': guildId,
                    'channel_id': channelId
                }
            );
        }
        catch (e) {
            Log.error(`Updating Tumblr for guild ${guildId}, channel ${channelId}, user ${tumblrUser}: ${e}`);
            throw e;
        }
    }
}

module.exports = TumblrCheckerRepository;