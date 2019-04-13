'use strict';

const moment = require('moment');

const NoisyInhibitor = require('../NoisyInhibitor.js');

class CommandCooldown extends NoisyInhibitor {
    async getBlockedMessage({ message, client }, command) {
        const { maxDailyUseCount } = command.command;
        if (maxDailyUseCount !== null) {
            const { registry } = client.master;
            const { author } = message;

            const dailyCount = await registry.cooldowns.getDailyUseCount(author, command);

            if (dailyCount >= maxDailyUseCount) {
                let uiSecondMessage;
                if (dailyCount < maxDailyUseCount + 5) {
                    await registry.cooldowns.addDailyUseCount(author, command);
                    uiSecondMessage = 'Continuing to use it will lead to you being ignored.';
                }
                else {
                    const duration = moment.duration(5, 'days');
                    await registry.users.ignoreUser(author, moment().add(duration).toDate());
                    uiSecondMessage = `You're abusing me, I will ignore you for **${duration.humanize()}**.`;
                }

                return {
                    ui: [
                        `You can't use \`${command.name}\` because you have exceeded the daily limit for this command (**${maxDailyUseCount}** times per day).`,
                        uiSecondMessage
                    ].join('\n'),
                    log: `They used it ${dailyCount} times today (max ${maxDailyUseCount}).`
                };
            }
        }

        return null;
    }
}

module.exports = CommandCooldown;