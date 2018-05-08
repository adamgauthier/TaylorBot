'use strict';

const { GlobalPaths } = require('globalobjects');

const Inhibitor = require(GlobalPaths.Inhibitor);
const Log = require(GlobalPaths.Logger);

class DisabledCommandInhibitor extends Inhibitor {
    shouldBeBlocked({ command }) {
        if (!command)
            return false;

        const cachedCommand = command.client.oldRegistry.commands.get(command.name);

        if (!cachedCommand.isEnabled) {
            Log.verbose(`Command '${command.name}' can't be used because it is disabled.`);
            return true;
        }

        return false;
    }
}

module.exports = DisabledCommandInhibitor;