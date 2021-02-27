import moment = require('moment');

import { NoisyInhibitor } from '../NoisyInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';

class CommandCooldown extends NoisyInhibitor {
    async getBlockedMessage({ author, client }: MessageContext, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
        const { maxDailyUseCount } = command.command;
        if (maxDailyUseCount !== null) {
            const { registry } = client.master;

            const dailyCount = await registry.cooldowns.getDailyUseCount(author, command);

            if (dailyCount >= maxDailyUseCount) {
                let uiSecondMessage;
                if (dailyCount < maxDailyUseCount + 7) {
                    await registry.cooldowns.addDailyUseCount(author, command);
                    uiSecondMessage = '**Stop using the command or I will ignore you.**';
                }
                else {
                    const duration = moment.duration(5, 'days');
                    await registry.users.ignoreUser(author, moment().add(duration).toDate());
                    uiSecondMessage = `**You're abusing me, I will ignore you for ${duration.humanize()}**.`;
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

export = CommandCooldown;
