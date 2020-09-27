import UserGroups = require('../../client/UserGroups.js');
import { Command } from '../Command';
import TimeUtil = require('../../modules/TimeUtil.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { User } from 'discord.js';
import { SherlockResult } from 'sherlockjs';

class IgnoreUserCommand extends Command {
    constructor() {
        super({
            name: 'ignoreuser',
            aliases: ['iu'],
            group: 'admin',
            description: 'Ignores a user for a period of time.',
            minimumGroup: UserGroups.Master,
            examples: ['@Enchanted13#1989 12 minutes'],

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'mentioned-user',
                    prompt: 'What users would you like me to ignore (must be mentioned)?'
                },
                {
                    key: 'event',
                    label: 'when',
                    type: 'future-event',
                    prompt: 'When should I stop ignoring this user?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { user, event }: { user: User; event: SherlockResult }): Promise<void> {
        const { channel } = message;
        const { users } = client.master.registry;

        await users.ignoreUser(user, event.startDate!);

        await client.sendEmbedSuccess(channel,
            `Okay, I will ignore ${user} until \`${TimeUtil.formatLog(event.startDate!.valueOf())}\`. 😊`
        );
    }
}

export = IgnoreUserCommand;
