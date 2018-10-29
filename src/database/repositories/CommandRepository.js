'use strict';

const Log = require('../../tools/Logger.js');

class CommandRepository {
    constructor(db, helpers) {
        this._db = db;
        this._helpers = helpers;
        this._columnSet = new this._helpers.ColumnSet(['name'], { table: 'commands.commands' });
    }

    async getAll() {
        try {
            return await this._db.instance.any('SELECT * FROM commands.commands;');
        }
        catch (e) {
            Log.error(`Getting all commands: ${e}`);
            throw e;
        }
    }

    async addAll(databaseCommands) {
        try {
            return await this._db.instance.any(
                `${this._helpers.insert(databaseCommands, this._columnSet)} RETURNING *;`,
            );
        }
        catch (e) {
            Log.error(`Adding commands: ${e}`);
            throw e;
        }
    }

    async setEnabled(commandName, enabled) {
        try {
            return await this._db.instance.oneOrNone(
                'UPDATE commands.commands SET enabled = $[enabled] WHERE name = $[name] RETURNING *;',
                {
                    'enabled': enabled,
                    'name': commandName
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