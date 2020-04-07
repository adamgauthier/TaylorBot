'use strict';

const Log = require('../../tools/Logger.js');

class CommandRepository {
    constructor(db, helpers) {
        this._db = db;
        this._helpers = helpers;
        this._columnSet = new this._helpers.ColumnSet(['name'], {
            table: new this._helpers.TableName('commands', 'commands')
        });
    }

    async getAll() {
        try {
            return await this._db.any('SELECT name, enabled FROM commands.commands;');
        }
        catch (e) {
            Log.error(`Getting all commands: ${e}`);
            throw e;
        }
    }

    async addAll(databaseCommands) {
        try {
            return await this._db.any(
                `${this._helpers.insert(databaseCommands, this._columnSet)} RETURNING name, enabled;`,
            );
        }
        catch (e) {
            Log.error(`Adding commands: ${e}`);
            throw e;
        }
    }

    async setEnabled(commandName, enabled) {
        try {
            return await this._db.one(
                'UPDATE commands.commands SET enabled = $[enabled] WHERE name = $[name] RETURNING *;',
                {
                    enabled: enabled,
                    name: commandName
                }
            );
        }
        catch (e) {
            Log.error(`Setting command '${commandName}' enabled to '${enabled}': ${e}`);
            throw e;
        }
    }

    async addUseCount(commandNames, useCount, errorCount) {
        try {
            return await this._db.none(
                `UPDATE commands.commands SET
                    successful_use_count = successful_use_count + $[use_count],
                    unhandled_error_count = unhandled_error_count + $[error_count]
                WHERE name IN ($[names:csv]);`,
                {
                    use_count: useCount,
                    error_count: errorCount,
                    names: commandNames
                }
            );
        }
        catch (e) {
            Log.error(`Adding ${useCount} use count and ${errorCount} error count to command '${commandNames.join()}': ${e}`);
            throw e;
        }
    }

    async insertOrGetIsCommandDisabled(command) {
        try {
            return await this._db.one(
                `INSERT INTO commands.commands (name, aliases) VALUES ($[command_name], $[aliases])
                ON CONFLICT (name) DO UPDATE SET
                    aliases = excluded.aliases
                RETURNING enabled;`,
                {
                    command_name: command.name,
                    aliases: command.command.aliases
                }
            );
        }
        catch (e) {
            Log.error(`Inserting or getting is disabled for ${command.name}: ${e}`);
            throw e;
        }
    }
}

module.exports = CommandRepository;
