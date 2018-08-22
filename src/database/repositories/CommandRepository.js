'use strict';

const Log = require('../../tools/Logger.js');

class CommandRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.commands.commands.find();
        }
        catch (e) {
            Log.error(`Getting all commands: ${e}`);
            throw e;
        }
    }

    async addAll(databaseCommands) {
        try {
            return await this._db.commands.commands.insert(databaseCommands);
        }
        catch (e) {
            Log.error(`Adding commands: ${e}`);
            throw e;
        }
    }

    async setEnabled(commandName, enabled) {
        try {
            return await this._db.commands.commands.update(
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