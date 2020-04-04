import moment = require('moment');

import TimeUtil = require('../../modules/TimeUtil.js');
import { SilentInhibitor } from '../SilentInhibitor';
import { MessageContext } from '../../structures/MessageContext';

class IgnoredInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: MessageContext): Promise<string | null> {
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
