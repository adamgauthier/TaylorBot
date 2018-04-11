'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);

class CommandReregister extends EventHandler {
    handler({ oldRegistry }, newCommand, oldCommand) {
        oldRegistry.commands.onReregister(newCommand, oldCommand);
    }
}

module.exports = CommandReregister;