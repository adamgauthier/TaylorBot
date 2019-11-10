'use strict';

const Format = require('../../modules/DiscordFormatter.js');

class CachedCommand {
    constructor(name, commandRegistry, guildCommandRepository) {
        this._commandRegistry = commandRegistry;
        this._guildCommandRepository = guildCommandRepository;

        this.name = name;
        this.disabledIn = {};
    }

    async setEnabled(enabled) {
        await this._commandRegistry.setGlobalEnabled(this.name, enabled);
    }

    enableCommand() {
        return this.setEnabled(true);
    }

    disableCommand() {
        return this.setEnabled(false);
    }

    async setGuildCommandEnabled(guild, enabled) {
        if (this.disabledIn[guild.id] && !enabled)
            throw new Error(`Command '${this.name}' is already disabled in ${Format.guild(guild)}.`);

        if (!this.disabledIn[guild.id] && enabled)
            throw new Error(`Command '${this.name}' is already enabled in ${Format.guild(guild)}.`);

        await this._guildCommandRepository.setDisabled(guild, this.name, !enabled);

        if (!enabled)
            this.disabledIn[guild.id] = true;
        else
            delete this.disabledIn[guild.id];
    }

    enableIn(guild) {
        return this.setGuildCommandEnabled(guild, true);
    }

    disableIn(guild) {
        return this.setGuildCommandEnabled(guild, false);
    }
}

module.exports = CachedCommand;
