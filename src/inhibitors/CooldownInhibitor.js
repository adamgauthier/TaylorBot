'use strict';

const { GlobalPaths } = require('globalobjects');

const Inhibitor = require(GlobalPaths.Inhibitor);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const TimeUtil = require(GlobalPaths.TimeUtil);

class CooldownInhibitor extends Inhibitor {
    shouldBeBlocked(message, command) {
        const { author, client } = message;

        const commandTime = new Date().getTime();
        const { lastCommand, lastAnswered, ignoreUntil } = client.oldRegistry.users.get(author.id);

        if (commandTime < ignoreUntil) {
            Log.verbose(`Command '${command.info.name}' can't be used by ${Format.user(author)} because they are ignored until ${TimeUtil.formatLog(ignoreUntil)}.`);
            return true;
        }

        if (lastAnswered < lastCommand) {
            Log.verbose(`Command '${command.info.name}' can't be used by ${Format.user(author)} because they have not been answered. LastAnswered:${lastAnswered}, LastCommand:${lastCommand}.`);
            return true;
        }

        return false;
    }
}

module.exports = CooldownInhibitor;