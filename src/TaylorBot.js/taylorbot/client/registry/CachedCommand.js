'use strict';

const Format = require('../../modules/DiscordFormatter.js');

class CachedCommand {
    constructor(name, commandRepository, guildCommandRepository) {
        this._commandRepository = commandRepository;
        this._guildCommandRepository = guildCommandRepository;

        this.name = name;
        this.isDisabled = false;
        this.disabledIn = {};
    }

    async setEnabled(enabled) {
        if (this.isDisabled && !enabled)
            throw new Error(`Command '${this.name}' is already disabled.`);

        if (!this.isDisabled && enabled)
            throw new Error(`Command '${this.name}' is already enabled.`);

        await this._commandRepository.setEnabled(this.name, enabled);

        this.isDisabled = !enabled;
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