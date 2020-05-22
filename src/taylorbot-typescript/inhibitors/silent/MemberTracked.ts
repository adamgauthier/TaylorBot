import Format = require('../../modules/DiscordFormatter.js');
import Log = require('../../tools/Logger.js');
import { SilentInhibitor } from '../SilentInhibitor.js';
import { MessageContext } from '../../structures/MessageContext.js';

class MemberTracked extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: MessageContext): Promise<string | null> {
        const { member } = message;

        if (!member)
            return null;

        const memberAdded = await client.master.registry.guilds.addOrUpdateMemberAsync(member, message.createdAt);

        if (memberAdded) {
            Log.verbose(`Added new member ${Format.member(member)}.`);
        }

        return null;
    }
}

export = MemberTracked;
