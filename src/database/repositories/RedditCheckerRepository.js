'use strict';

const Log = require('../../tools/Logger.js');

class RedditCheckerRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.instance.any('SELECT * FROM checkers.reddit_checker;');
        }
        catch (e) {
            Log.error(`Getting Reddits: ${e}`);
            throw e;
        }
    }

    async update(subreddit, guildId, channelId, lastLink, lastCreated) {
        try {
            return await this._db.instance.oneOrNone(
                `UPDATE checkers.reddit_checker SET last_post_id = $[last_post_id], last_created = $[last_created]
                WHERE subreddit = $[subreddit] AND guild_id = $[guild_id] AND channel_id = $[channel_id]
                RETURNING *;`,
                {
                    'last_post_id': lastLink,
                    'last_created': lastCreated,
                    'subreddit': subreddit,
                    'guild_id': guildId,
                    'channel_id': channelId
                }
            );
        }
        catch (e) {
            Log.error(`Updating Reddit for guild ${guildId}, channel ${channelId}, subreddit ${subreddit}: ${e}`);
            throw e;
        }
    }
}

module.exports = RedditCheckerRepository;