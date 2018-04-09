'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);

class CommandPrefixChange extends EventHandler {
    handler({ oldRegistry }, guild, prefix) {
        if (guild && prefix)
            oldRegistry.guilds.changePrefix(guild, prefix);
    }
}

module.exports = CommandPrefixChange;