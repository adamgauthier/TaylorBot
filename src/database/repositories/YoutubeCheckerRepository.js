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
            return [];
        }
    }
}

module.exports = YoutubeCheckerRepository;