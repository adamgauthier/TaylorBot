'use strict';

const SilentInhibitor = require('../SilentInhibitor.js');

class DisabledGuildCommandInhibitor extends SilentInhibitor {
    shouldBeBlocked({ message }, command) {
        const { guild } = message;

        if (!guild)
            return null;

        if (command.disabledIn[guild.id]) {
            return 'The command is disabled in this guild.';
        }

        return null;
    }
}

module.exports = DisabledGuildCommandInhibitor;