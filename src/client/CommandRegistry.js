'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

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
            this.set(command.name, {
                'disabledIn': {}
            });
        }

        databaseCommands.forEach(c => {
            const command = registry.resolveCommand(c.name);
            const cachedCommand = this.get(command.name);
            if (!c.enabled)
                cachedCommand.isDisabled = true;
        });

        const guildCommands = await database.guildCommands.getAll();
        guildCommands.forEach(gc => {
            const command = registry.resolveCommand(gc.command_name);
            if (gc.disabled) {
                this.get(command.name).disabledIn[gc.guild_id] = true;
            }
        });
    }

    async setCommandEnabled(command, enabled) {
        const cachedCommand = this.get(command.name);

        if (!cachedCommand)
            throw new Error(`Command '${command.name}' wasn't cached when trying to set enabled to ${enabled}.`);

        if (cachedCommand.isDisabled && !enabled)
            throw new Error(`Command '${command.name}' is already disabled.`);

        if (!cachedCommand.isDisabled && enabled)
            throw new Error(`Command '${command.name}' is already enabled.`);

        await this.client.master.database.commands.setEnabled(command.name, enabled);

        if (!enabled)
            cachedCommand.isDisabled = true;
        else
            delete cachedCommand.isDisabled;
    }

    enableCommand(command) {
        return this.setCommandEnabled(command, true);
    }

    disableCommand(command) {
        return this.setCommandEnabled(command, false);
    }

    async setGuildCommandEnabled(guild, command, enabled) {
        const cachedCommand = this.get(command.name);

        if (!cachedCommand)
            throw new Error(`Command '${command.name}' wasn't cached when trying to set enabled to ${enabled} in ${Format.guild(guild)}.`);

        if (cachedCommand.disabledIn[guild.id] && !enabled)
            throw new Error(`Command '${command.name}' is already disabled in ${Format.guild(guild)}.`);

        if (!cachedCommand.disabledIn[guild.id] && enabled)
            throw new Error(`Command '${command.name}' is already enabled in ${Format.guild(guild)}.`);

        await this.client.master.database.guildCommands.setDisabled(guild, command.name, !enabled);

        if (!enabled)
            cachedCommand.disabledIn[guild.id] = true;
        else
            delete cachedCommand.disabledIn[guild.id];
    }

    enableCommandIn(command, guild) {
        return this.setGuildCommandEnabled(guild, command, true);
    }

    disableCommandIn(command, guild) {
        return this.setGuildCommandEnabled(guild, command, false);
    }
}

module.exports = CommandRegistry;