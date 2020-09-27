import moment = require('moment');

import { TimeUtil } from '../../modules/util/TimeUtil';
import { SilentInhibitor } from '../SilentInhibitor';
import { MessageContext } from '../../structures/MessageContext';

class IgnoredInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ author, client }: MessageContext): Promise<string | null> {
        const ignoredUntil = await client.master.registry.users.getIgnoredUntil(author);

        const commandTime = moment();
        if (commandTime.isBefore(ignoredUntil)) {
            return `They are ignored until ${TimeUtil.formatLog(ignoredUntil.valueOf())}.`;
        }

        return null;
    }
}

export = IgnoredInhibitor;
