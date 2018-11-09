'use strict';

const Inhibitor = require('../structures/Inhibitor.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class OldAttributesInhibitor extends Inhibitor {
    shouldBeBlocked({ message }, command, argString) {
        const { author } = message;

        if (command.name === 'rank' &&
            ['minutes', 'points', 'rolls', '1989rolls'].includes(argString.trim().split(' ')[0])) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because it uses old attributes.`);
            return true;
        }

        return false;
    }
}

module.exports = OldAttributesInhibitor;