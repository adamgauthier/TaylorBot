import moment = require('moment');

import TimeUtil = require('../../modules/TimeUtil.js');
import { SilentInhibitor } from '../SilentInhibitor';
import { Message } from 'discord.js';
import { TaylorBotClient } from '../../client/TaylorBotClient.js';

class IgnoredInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: { message: Message; client: TaylorBotClient }): Promise<string | null> {
        const { author } = message;

        const ignoredUntil = await client.master.registry.users.getIgnoredUntil(author);

        const commandTime = moment();
        if (commandTime.isBefore(ignoredUntil)) {
            return `They are ignored until ${TimeUtil.formatLog(ignoredUntil.valueOf())}.`;
        }

        return null;
    }
}

export = IgnoredInhibitor;
