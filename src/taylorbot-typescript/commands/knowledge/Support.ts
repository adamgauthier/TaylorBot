import patreonConfig = require('../../config/patreon.json');

import { Command } from '../Command';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { CommandMessageContext } from '../CommandMessageContext';

class SupportCommand extends Command {
    constructor() {
        super({
            name: 'support',
            aliases: ['donate', 'patreon'],
            group: 'Knowledge ❓',
            description: 'Gives information about where you can support TaylorBot!',
            examples: [''],

            args: []
        });
    }

    async run({ message, client }: CommandMessageContext): Promise<void> {
        const { channel } = message;

        await client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserHeader(client.user!)
            .setColor(patreonConfig.COLOR)
            .setTitle(patreonConfig.TITLE)
            .setDescription(patreonConfig.DESCRIPTION)
            .setURL(patreonConfig.URL)
            .setThumbnail(patreonConfig.THUMBNAIL)
        );
    }
}

export = SupportCommand;
