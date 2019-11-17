'use strict';

class CachedCommand {
    constructor(name, commandRegistry) {
        this._commandRegistry = commandRegistry;

        this.name = name;
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

    setGuildCommandEnabled(guild, enabled) {
        return this._commandRegistry.setGuildEnabled(guild, this.name, enabled);
    }

    enableIn(guild) {
        return this.setGuildCommandEnabled(guild, true);
    }

    disableIn(guild) {
        return this.setGuildCommandEnabled(guild, false);
    }
}

module.exports = CachedCommand;
