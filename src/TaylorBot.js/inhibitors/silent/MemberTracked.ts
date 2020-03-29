import Format = require('../../modules/DiscordFormatter.js');
import Log = require('../../tools/Logger.js');
import { SilentInhibitor } from '../SilentInhibitor.js';
import { Message } from 'discord.js';
import { TaylorBotClient } from '../../client/TaylorBotClient.js';

class MemberTracked extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: { message: Message; client: TaylorBotClient }): Promise<string | null> {
        const { member } = message;

        if (!member)
            return null;

        const memberAdded = await client.master.registry.guilds.addOrUpdateMemberAsync(member);

        if (memberAdded) {
            Log.verbose(`Added new member ${Format.member(member)}.`);
        }

        return null;
    }
}

export = MemberTracked;
