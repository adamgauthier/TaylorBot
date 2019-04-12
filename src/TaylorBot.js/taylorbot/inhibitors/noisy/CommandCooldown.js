'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');

class CommandCooldown extends NoisyInhibitor {
    async getBlockedMessage({ message, client }, command) {
        const { maxDailyUseCount } = command.command;
        if (maxDailyUseCount !== null) {
            const { author } = message;

            const dailyCount = await client.master.registry.cooldowns.getDailyUseCount(author, command);

            if (dailyCount >= maxDailyUseCount) {
                return {
                    ui: `You can't use \`${command.name}\` because you have exceeded the maximum daily use count for this command (**${maxDailyUseCount}** times per day).`,
                    log: `They used it ${dailyCount} times today (max ${maxDailyUseCount}).`
                };
            }
        }

        return null;
    }
}

module.exports = CommandCooldown;