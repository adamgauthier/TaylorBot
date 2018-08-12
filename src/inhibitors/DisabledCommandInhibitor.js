'use strict';

const { Paths } = require('globalobjects');

const Inhibitor = require(Paths.Inhibitor);
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