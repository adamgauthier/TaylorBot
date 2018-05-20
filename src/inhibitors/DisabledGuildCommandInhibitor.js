'use strict';

const { Paths } = require('globalobjects');

const Inhibitor = require(Paths.Inhibitor);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

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