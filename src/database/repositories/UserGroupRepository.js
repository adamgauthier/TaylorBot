'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class UserGroupRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.user_groups.find();
        }
        catch (e) {
            Log.error(`Getting all user groups: ${e}`);
            throw e;
        }
    }

    async addAll(userGroups) {
        try {
            return await this._db.user_groups.insert(userGroups);
        }
        catch (e) {
            Log.error(`Adding user groups: ${e}`);
            throw e;
        }
    }
}

module.exports = UserGroupRepository;