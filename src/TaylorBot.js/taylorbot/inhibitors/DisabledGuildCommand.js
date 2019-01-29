'use strict';

const Inhibitor = require('../structures/Inhibitor.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class DisabledGuildCommandInhibitor extends Inhibitor {
    shouldBeBlocked({ message }, command) {
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