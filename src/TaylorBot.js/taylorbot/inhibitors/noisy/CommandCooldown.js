'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');
const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class CommandCooldown extends NoisyInhibitor {
    async getBlockedMessage({ message, client }, command) {
        const { maxDailyUseCount } = command.command;
        if (maxDailyUseCount !== null) {
            const { author } = message;

            const dailyCount = await client.master.registry.cooldowns.getDailyUseCount(author, command);

            if (dailyCount >= maxDailyUseCount) {
                Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they have used it ${dailyCount} times today (max ${maxDailyUseCount}).`);
                return `You can't use \`${command.name}\` because you have exceeded the maximum daily use count for this command (**${maxDailyUseCount}** times per day).`;
            }
        }

        return null;
    }
}

module.exports = CommandCooldown;