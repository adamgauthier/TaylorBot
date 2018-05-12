'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const CachedCommand = require(GlobalPaths.CachedCommand);

class CommandRegistry extends Map {
    constructor(client) {
        super();
        this.client = client;
    }

    async loadAll() {
        const { registry } = this.client;
        const { database } = this.client.master;
        registry.registerCommandsIn(GlobalPaths.commandsFolderPath);

        let databaseCommands = await database.commands.getAll();

        const { commands } = registry;

        const databaseCommandsNotInFiles = databaseCommands.filter(c =>
            !commands.has(c.name)
        );
        if (databaseCommandsNotInFiles.length > 0)
            throw new Error(`Found database commands not in files: ${databaseCommandsNotInFiles.join(',')}.`);

        const fileCommandsNotInDatabase = commands.filterArray(command =>
            !databaseCommands.some(c => c.name === command.name) && !command.guarded
        );

        if (fileCommandsNotInDatabase.length > 0) {
            Log.info(`Found new file commands ${fileCommandsNotInDatabase.map(c => c.name).join(',')}. Adding to database.`);

            await database.commands.addAll(
                fileCommandsNotInDatabase.map(command => {
                    return { 'name': command.name };
                })
            );

            databaseCommands = await database.commands.getAll();
        }

        for (const command of commands.values()) {
            this.set(command.name, new CachedCommand(
                command.name,
                this.client.master.database.commands,
                this.client.master.database.guildCommands
            ));
        }

        databaseCommands.forEach(c => {
            if (!c.enabled)
                this.getCommand(c.name).isDisabled = true;
        });

        const guildCommands = await database.guildCommands.getAll();
        guildCommands.forEach(gc => {
            if (gc.disabled)
                this.getCommand(gc.command_name).disabledIn[gc.guild_id] = true;
        });
    }

    getCommand(name) {
        const cachedCommand = this.get(name);

        if (!cachedCommand)
            throw new Error(`Command '${name}' isn't cached.`);

        return cachedCommand;
    }
}

module.exports = CommandRegistry;