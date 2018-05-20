'use strict';

const { Paths } = require('globalobjects');

const Log = require(Paths.Logger);

class RedditCheckerRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.checkers.reddit_checker.find();
        }
        catch (e) {
            Log.error(`Getting Reddits: ${e}`);
            throw e;
        }
    }

    async update(subreddit, guildId, channelId, lastLink, lastCreated) {
        try {
            return await this._db.checkers.reddit_checker.update(
                {
                    'subreddit': subreddit,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_post_id': lastLink,
                    'last_created': lastCreated
                },
                {
                    'single': true
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