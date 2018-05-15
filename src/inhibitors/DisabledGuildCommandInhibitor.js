'use strict';

const { GlobalPaths } = require('globalobjects');

const Inhibitor = require(GlobalPaths.Inhibitor);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class DisabledGuildCommandInhibitor extends Inhibitor {
    shouldBeBlocked(message, command) {
        const { guild } = message;

        if (!guild)
            return false;

        if (command.disabledIn[guild.id]) {
            Log.verbose(`Command '${command.name}' can't be used in ${Format.guild(guild)} because it is disabled.`);
            return true;
        }

        return false;
    }
}

module.exports = DisabledGuildCommandInhibitor;