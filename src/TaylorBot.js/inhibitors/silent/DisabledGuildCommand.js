'use strict';

const SilentInhibitor = require('../SilentInhibitor.js');

class DisabledGuildCommandInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }, command) {
        const { guild } = message;

        if (!guild)
            return null;

        const isDisabled = await client.master.registry.commands.getIsGuildCommandDisabled(guild, command);

        if (isDisabled) {
            return 'The command is disabled in this guild.';
        }

        return null;
    }
}

module.exports = DisabledGuildCommandInhibitor;
