'use strict';

const { GlobalPaths } = require('globalobjects');

const Inhibitor = require(GlobalPaths.Inhibitor);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class CooldownInhibitor extends Inhibitor {
    shouldBeBlocked({ message, command }) {
        if (!command)
            return false;

        const { author } = message;

        const commandTime = new Date().getTime();
        const { lastCommand, lastAnswered, ignoreUntil } = command.client.oldRegistry.users.get(author.id);

        if (commandTime < ignoreUntil) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they are ignored until ${moment(ignoreUntil, 'x').format('MMM Do YY, H:mm:ss Z')}.`);
            return true;
        }

        if (lastAnswered < lastCommand) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they have not been answered. LastAnswered:${lastAnswered}, LastCommand:${lastCommand}.`);
            return true;
        }
    }
}

module.exports = CooldownInhibitor;