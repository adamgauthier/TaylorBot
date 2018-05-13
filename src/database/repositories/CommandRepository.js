'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class CommandRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.commands.find();
        }
        catch (e) {
            Log.error(`Getting all commands: ${e}`);
            throw e;
        }
    }

    async addAll(databaseCommands) {
        try {
            return await this._db.commands.insert(databaseCommands);
        }
        catch (e) {
            Log.error(`Adding commands: ${e}`);
            throw e;
        }
    }

    async setEnabled(commandName, enabled) {
        try {
            return await this._db.commands.update(
                {
                    'name': commandName
                },
                {
                    'enabled': enabled
                },
                {
                    'single': true
                }
            );
        }
        catch (e) {
            Log.error(`Setting command '${commandName}' enabled to '${enabled}': ${e}`);
            throw e;
        }
    }
}

module.exports = CommandRepository;