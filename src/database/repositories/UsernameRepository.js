'use strict';

const { Paths } = require('globalobjects');

const Log = require('../../tools/Logger.js');
const Format = require(Paths.DiscordFormatter);

class UsernameRepository {
    constructor(db) {
        this._db = db;
    }

    async getAllLatest() {
        try {
            return await this._db.usernames.getLatestUsernames();
        }
        catch (e) {
            Log.error(`Getting all usernames: ${e}`);
            throw e;
        }
    }

    async getLatest(user) {
        try {
            return await this._db.usernames.getLatestUsername(
                {
                    'user_id': user.id
                },
                {
                    'single': true
                }
            );
        }
        catch (e) {
            Log.error(`Getting latest username for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    mapUserToUsernameDatabase(user, changedAt) {
        return {
            'user_id': user.id,
            'username': user.username,
            'changed_at': changedAt
        };
    }

    async add(user, changedAt) {
        const databaseUsername = this.mapUserToUsernameDatabase(user, changedAt);
        try {
            return await this._db.usernames.insert(databaseUsername);
        }
        catch (e) {
            Log.error(`Adding username for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async getHistory(user, limit) {
        try {
            return await this._db.usernames.getUsernameHistory(
                {
                    'user_id': user.id,
                    'max_rows': limit
                }
            );
        }
        catch (e) {
            Log.error(`Getting username history for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = UsernameRepository;