'use strict';

const { GlobalPaths } = require('globalobjects');

const Inhibitor = require(GlobalPaths.Inhibitor);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class DisabledGuildCommandInhibitor extends Inhibitor {
    shouldBeBlocked({ message, command }) {
        if (!command)
            return false;

        const { guild } = message;

        if (!guild)
            return false;

        const cachedCommand = command.client.oldRegistry.commands.get(command.name);

        if (cachedCommand.disabledIn[guild.id]) {
            Log.verbose(`Command '${command.name}' can't be used in ${Format.guild(guild)} because it is disabled.`);
            return true;
        }

        return false;
    }
}

module.exports = DisabledGuildCommandInhibitor;