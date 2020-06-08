import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import Command = require('../Command.js');
import TimeUtil = require('../../modules/TimeUtil.js');
import { ArrayUtil } from '../../modules/util/ArrayUtil';
import PageMessage = require('../../modules/paging/PageMessage.js');
import EmbedDescriptionPageEditor = require('../../modules/paging/editors/EmbedDescriptionPageEditor.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { User } from 'discord.js';

class UsernamesCommand extends Command {
    constructor() {
        super({
            name: 'usernames',
            aliases: ['names'],
            group: 'Stats 📊',
            description: 'Gets a list of previous usernames for a user.',
            examples: ['@Enchanted13#1989', 'Enchanted13'],

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'user-or-author',
                    prompt: 'What user would you like to see the usernames history of?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { user }: { user: User }): Promise<void> {
        const { channel, author } = message;
        const usernames = await client.master.database.usernames.getHistory(user, 75);
        const embed = DiscordEmbedFormatter.baseUserEmbed(user);

        const lines = usernames.map(u => `${TimeUtil.formatSmall(u.changed_at.getTime())} : ${u.username}`);
        const chunks = ArrayUtil.chunk(lines, 15);

        return new PageMessage(
            client,
            author,
            chunks.map(chunk => chunk.join('\n')),
            new EmbedDescriptionPageEditor(embed)
        ).send(channel);
    }
}

export = UsernamesCommand;
