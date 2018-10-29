'use strict';

const Log = require('../../tools/Logger.js');

class UserGroupRepository {
    constructor(db, helpers) {
        this._db = db;
        this._helpers = helpers;
        this._columnSet = new this._helpers.ColumnSet(['name', 'access_level'], { table: 'commands.user_groups' });
    }

    async getAll() {
        try {
            return await this._db.any('SELECT * FROM commands.user_groups;');
        }
        catch (e) {
            Log.error(`Getting all user groups: ${e}`);
            throw e;
        }
    }

    async addAll(userGroups) {
        try {
            return await this._db.any(
                `${this._helpers.insert(userGroups, this._columnSet)} RETURNING *;`,
            );
        }
        catch (e) {
            Log.error(`Adding user groups: ${e}`);
            throw e;
        }
    }
}

module.exports = UserGroupRepository;