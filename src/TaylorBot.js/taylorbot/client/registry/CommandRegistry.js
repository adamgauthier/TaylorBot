'use strict';

const Log = require('../../tools/Logger.js');
const CachedCommand = require('./CachedCommand.js');
const CommandLoader = require('../../commands/CommandLoader.js');
const AttributeLoader = require('../../attributes/AttributeLoader.js');

class CommandRegistry {
    constructor(database) {
        this.database = database;
        this.commandsCache = new Map();
        this.useCountCache = new Map();
    }

    async loadAll() {
        const databaseCommands = await this.database.commands.getAll();

        const commands = [
            ...(await CommandLoader.loadAll()),
            ...(await AttributeLoader.loadMemberAttributeCommands()),
            ...(await AttributeLoader.loadUserAttributeCommands())
        ];

        const fileCommandsNotInDatabase = commands.filter(command =>
            !databaseCommands.some(c => c.name === command.name)
        );

        if (fileCommandsNotInDatabase.length > 0) {
            Log.info(`Found new file commands ${fileCommandsNotInDatabase.map(c => c.name).join(',')}. Adding to database.`);

            const inserted = await this.database.commands.addAll(
                fileCommandsNotInDatabase.map(command => {
                    return { 'name': command.name };
                })
            );

            databaseCommands.push(...inserted);
        }

        commands.forEach(c => this.cacheCommand(c));

        databaseCommands.forEach(c => {
            if (!c.enabled)
                this.getCommand(c.name).isDisabled = true;
        });

        const guildCommands = await this.database.guildCommands.getAll();
        guildCommands.forEach(gc => {
            if (gc.disabled)
                this.getCommand(gc.command_name).disabledIn[gc.guild_id] = true;
        });
    }

    cacheCommand(command) {
        const key = command.name.toLowerCase();

        if (this.commandsCache.has(key))
            throw new Error(`Command '${command.name}' is already cached.`);

        const cached = new CachedCommand(
            command.name,
            this.database.commands,
            this.database.guildCommands
        );
        cached.command = command;

        this.commandsCache.set(key, cached);

        for (const alias of command.aliases) {
            const aliasKey = alias.toLowerCase();

            if (this.commandsCache.has(aliasKey))
                throw new Error(`Command Key '${aliasKey}' is already cached when setting alias.`);

            this.commandsCache.set(aliasKey, key);
        }
    }

    getCommand(name) {
        const cachedCommand = this.commandsCache.get(name);

        if (!cachedCommand)
            throw new Error(`Command '${name}' isn't cached.`);

        if (typeof (cachedCommand) === 'string')
            throw new Error(`Command '${name}' is cached as an alias.`);

        return cachedCommand;
    }

    resolve(commandName) {
        const command = this.commandsCache.get(commandName.toLowerCase());

        if (typeof (command) === 'string') {
            return this.getCommand(command);
        }

        return command;
    }

    addSuccessfulUseCount(command) {
        const useCount = this.useCountCache.get(command.name);
        if (!useCount) {
            this.useCountCache.set(command.name, { count: 1, errorCount: 0 });
        }
        else {
            useCount.count += 1;
        }
    }

    addUnhandledErrorCount(command) {
        const useCount = this.useCountCache.get(command.name);
        if (!useCount) {
            this.useCountCache.set(command.name, { count: 0, errorCount: 1 });
        }
        else {
            useCount.errorCount += 1;
        }
    }

    getAllCommands() {
        return Array.from(
            this.commandsCache.values()
        ).filter(val => typeof (val) !== 'string');
    }
}

module.exports = CommandRegistry;
