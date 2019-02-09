'use strict';

const Inhibitor = require('../structures/Inhibitor.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class CommandCooldown extends Inhibitor {
    async shouldBeBlocked({ message, client }, command) {
        const { maxDailyUseCount } = command.command;
        if (maxDailyUseCount === null) {
            return false;
        }

        const { author } = message;

        const dailyCount = await client.master.registry.cooldowns.getDailyUseCount(author, command);

        if (dailyCount >= maxDailyUseCount) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they have used it ${dailyCount} times today (max ${maxDailyUseCount}).`);
            return true;
        }

        return false;
    }
}

module.exports = CommandCooldown;