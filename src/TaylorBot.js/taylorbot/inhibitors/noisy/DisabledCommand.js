'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');

class DisabledCommandInhibitor extends NoisyInhibitor {
    getBlockedMessage(messageContext, command) {
        if (command.isDisabled) {
            return {
                ui: `You can't use \`${command.name}\` because it is globally disabled right now. Please check back later.`,
                log: 'The command is disabled globally.'
            };
        }

        return null;
    }
}

module.exports = DisabledCommandInhibitor;