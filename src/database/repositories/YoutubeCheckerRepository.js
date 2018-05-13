'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class YoutubeCheckerRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.checkers.youtube_checker.find();
        }
        catch (e) {
            Log.error(`Getting Youtube Channels: ${e}`);
            throw e;
        }
    }

    async update(playlistId, guildId, channelId, lastVideoId) {
        try {
            return await this._db.checkers.youtube_checker.update(
                {
                    'playlist_id': playlistId,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_video_id': lastVideoId
                },
                {
                    'single': true
                }
            );
        }
        catch (e) {
            Log.error(`Updating Youtube for guild ${guildId}, channel ${channelId}, playlistId ${playlistId}: ${e}`);
            throw e;
        }
    }
}

module.exports = YoutubeCheckerRepository;