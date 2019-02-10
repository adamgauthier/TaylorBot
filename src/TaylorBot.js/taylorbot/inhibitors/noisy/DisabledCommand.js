'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');
const Log = require('../../tools/Logger.js');

class DisabledCommandInhibitor extends NoisyInhibitor {
    getBlockedMessage(messageContext, command) {
        if (command.isDisabled) {
            Log.verbose(`Command '${command.name}' can't be used because it is disabled.`);
            return `You can't use \`${command.name}\` because it is globally disabled right now. Please check back later.`;
        }

        return null;
    }
}

module.exports = DisabledCommandInhibitor;