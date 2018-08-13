'use strict';

const Inhibitor = require('../structures/Inhibitor.js');
const Log = require('../tools/Logger.js');

class DisabledCommandInhibitor extends Inhibitor {
    shouldBeBlocked(messageContext, command) {
        if (command.isDisabled) {
            Log.verbose(`Command '${command.name}' can't be used because it is disabled.`);
            return true;
        }

        return false;
    }
}

module.exports = DisabledCommandInhibitor;