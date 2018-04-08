'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class CommandRegistry {
    constructor(client) {
        this.client = client;
    }

    async loadAll() {
        const { registry, database } = this.client;
        registry.registerCommandsIn(GlobalPaths.commandsFolderPath);

        let databaseCommands = await database.getAllCommands();

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

            await database.addCommands(
                fileCommandsNotInDatabase.map(command => {
                    return { 'name': command.name };
                })
            );

            databaseCommands = await database.getAllCommands();
        }

        for (const command of commands.values()) {
            command.disabledIn = {};
        }

        databaseCommands.forEach(c => {
            const command = registry.resolveCommand(c.name);
            command._globalEnabled = c.enabled;
        });

        const guildCommands = await database.getAllGuildCommands();
        guildCommands.forEach(gc => {
            const command = registry.resolveCommand(gc.command_name);
            if (gc.disabled) {
                command.disabledIn[gc.guild_id] = true;
            }
        });
    }

    onReregister(newCommand, oldCommand) {
        newCommand.disabledIn = oldCommand.disabledIn;
    }

    syncDisabledGuildCommands() {
        const { registry, guilds } = this.client;
        for (const command of registry.commands.values()) {
            for (const guildId in command.disabledIn) {
                const guild = guilds.resolve(guildId);
                if (!guild._commandsEnabled)
                    guild._commandsEnabled = {};
                guild._commandsEnabled[command.name] = false;
            }
        }
    }

    setCommandEnabled(command, enabled) {
        return this.client.database.setCommandEnabled(command.name, enabled);
    }

    async setGuildCommandEnabled(guild, command, enabled) {
        await this.client.database.setGuildCommandDisabled(guild, command.name, !enabled);
        command.disabledIn[guild.id] = !enabled;
    }
}

module.exports = CommandRegistry;