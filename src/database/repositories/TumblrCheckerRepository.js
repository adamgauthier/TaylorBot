'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class TumblrCheckerRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.checkers.tumblr_checker.find();
        }
        catch (e) {
            Log.error(`Getting Tumblrs: ${e}`);
            throw e;
        }
    }

    async update(tumblrUser, guildId, channelId, lastLink) {
        try {
            return await this._db.checkers.tumblr_checker.update(
                {
                    'tumblr_user': tumblrUser,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_link': lastLink
                },
                {
                    'single': true
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