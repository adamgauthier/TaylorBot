'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class CommandSettings extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async load() {
        const commands = await this.database.getAllCommands();
        commands.forEach(c => this.cacheDatabaseCommand(c));

        const guildCommands = await this.database.getAllGuildCommands();
        guildCommands.forEach(gc => this.cacheGuildCommand(gc));
    }

    cacheCommand(name, fileCommand) {
        if (this.has(name))
            Log.warn(`Caching command ${name}, was already cached, overwriting.`);

        this.set(name, fileCommand);
    }

    cacheDatabaseCommand(databaseCommand) {
        const command = this.get(databaseCommand.name);
        if (command !== undefined) {
            command.enabled = databaseCommand.enabled;
            command.disabledIn = {};

            this.set(databaseCommand.name, command);
        }
        else {
            const error = `Caching database command ${databaseCommand.name}, command was not already cached.`;
            Log.error(error);
            throw new Error(error);
        }
    }

    cacheGuildCommand(databaseGuildCommand) {
        const command = this.get(databaseGuildCommand.command_name);
        if (command !== undefined) {
            command.disabledIn[databaseGuildCommand.guild_id] = true;

            this.set(databaseGuildCommand.command_name, command);
        }
        else {
            const error = `Caching guild command ${databaseGuildCommand.command_name} (${databaseGuildCommand.guild_id}), command was not already cached.`;
            Log.error(error);
            throw new Error(error);
        }
    }
}

module.exports = CommandSettings;