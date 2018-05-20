'use strict';

const { Paths } = require('globalobjects');

const Inhibitor = require(Paths.Inhibitor);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);
const TimeUtil = require(Paths.TimeUtil);

class CooldownInhibitor extends Inhibitor {
    shouldBeBlocked(message, command) {
        const { author, client } = message;

        const commandTime = new Date().getTime();
        const { lastCommand, lastAnswered, ignoreUntil } = client.master.registry.users.get(author.id);

        if (commandTime < ignoreUntil) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they are ignored until ${TimeUtil.formatLog(ignoreUntil)}.`);
            return true;
        }

        if (lastAnswered < lastCommand) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they have not been answered. LastAnswered:${lastAnswered}, LastCommand:${lastCommand}.`);
            return true;
        }

        return false;
    }
}

module.exports = CooldownInhibitor;