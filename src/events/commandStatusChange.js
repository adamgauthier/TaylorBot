'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class CommandStatusChange extends EventHandler {
    handler({ oldRegistry }, guild, command, enabled) {
        Log.verbose(`Command '${command.name}' enabled changed to ${enabled}${guild ? ` in guild ${Format.guild(guild)}` : ''}.`);

        if (guild)
            oldRegistry.commands.setGuildCommandEnabled(guild, command, enabled);
        else
            oldRegistry.commands.setCommandEnabled(command, enabled);
    }
}

module.exports = CommandStatusChange;