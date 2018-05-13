'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);

class CommandPrefixChange extends EventHandler {
    handler({ oldRegistry }, guild, prefix) {
        if (guild && prefix) {
            const cachedGuild = oldRegistry.guilds.get(guild.id);

            if (cachedGuild.prefix !== prefix) {
                oldRegistry.guilds.changePrefix(guild, prefix);
            }
        }
    }
}

module.exports = CommandPrefixChange;