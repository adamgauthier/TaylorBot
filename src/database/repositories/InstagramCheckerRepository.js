'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class InstagramCheckerRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.checkers.instagram_checker.find();
        }
        catch (e) {
            Log.error(`Getting Instagrams: ${e}`);
            throw e;
        }
    }

    async update(instagramUsername, guildId, channelId, lastCode) {
        try {
            return await this._db.checkers.instagram_checker.update(
                {
                    'instagram_username': instagramUsername,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_post_code': lastCode
                },
                {
                    'single': true
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