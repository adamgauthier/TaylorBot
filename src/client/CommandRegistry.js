'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const CommandLoader = require(GlobalPaths.CommandLoader);

class CommandRegistry extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async loadAll(client) {
        const fileCommands = await CommandLoader.loadAll(client);
        let commands = await this.database.getAllCommands();

        const fileCommandNames = Object.keys(fileCommands);

        const databaseCommandsNotInFiles = commands.filter(c =>
            !fileCommandNames.some(name => name === c.name)
        );
        if (databaseCommandsNotInFiles.length > 0)
            throw new Error(`Found database commands not in files: ${databaseCommandsNotInFiles.join(',')}.`);

        const fileCommandsNotInDatabase = fileCommandNames.filter(name =>
            !commands.some(c => c.name === name)
        );

        if (fileCommandsNotInDatabase.length > 0) {
            Log.info(`Found new file commands ${fileCommandsNotInDatabase.join(',')}. Adding to database.`);

            await this.database.addCommands(
                fileCommandsNotInDatabase.map(name => {
                    return { 'name': name };
                })
            );

            commands = await this.database.getAllCommands();
        }

        fileCommandNames.forEach(name => {
            const { aliases, ...command } = fileCommands[name];
            this.cacheCommand(name, command);
            aliases.forEach(alias => this.cacheAlias(alias, name));
        });

        commands.forEach(c => this.cacheDatabaseCommand(c));

        const guildCommands = await this.database.getAllGuildCommands();
        guildCommands.forEach(gc => this.cacheGuildCommand(gc));
    }

    cacheCommand(name, fileCommand) {
        if (this.has(name))
            Log.warn(`Caching command ${name}, was already cached, overwriting.`);

        this.set(name, fileCommand);
    }

    cacheAlias(alias, commandName) {
        if (!this.has(commandName))
            throw new Error(`Can't cache alias of command ${commandName}, because it is not cached.`);

        if (this.has(alias))
            throw new Error(`Can't cache alias ${alias} for ${commandName} because it is already cached.`);

        this.set(alias, commandName);
    }

    cacheDatabaseCommand(databaseCommand) {
        const command = this.get(databaseCommand.name);
        if (command === undefined)
            throw new Error(`Caching database command ${databaseCommand.name}, command was not already cached.`);

        command.enabled = databaseCommand.enabled;
        command.disabledIn = {};

        this.set(databaseCommand.name, command);
    }

    cacheGuildCommand(databaseGuildCommand) {
        const command = this.get(databaseGuildCommand.command_name);
        if (command === undefined)
            throw new Error(`Caching guild command ${databaseGuildCommand.command_name} (${databaseGuildCommand.guild_id}), command was not already cached.`);

        command.disabledIn[databaseGuildCommand.guild_id] = true;

        this.set(databaseGuildCommand.command_name, command);
    }

    getCommand(name) {
        let command = this.get(name);
        if (typeof command === 'string')
            return this.get(command);

        return command;
    }
}

module.exports = CommandRegistry;