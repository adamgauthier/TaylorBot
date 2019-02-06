'use strict';

const patreonConfig = require('../../config/patreon.json');

const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

class SupportCommand extends Command {
    constructor() {
        super({
            name: 'support',
            aliases: ['donate', 'patreon'],
            group: 'knowledge',
            description: 'Gives information about where you can support TaylorBot!',
            examples: [''],

            args: []
        });
    }

    async run({ message, client }) {
        const { channel } = message;

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserHeader(client.user)
            .setColor(patreonConfig.COLOR)
            .setTitle(patreonConfig.TITLE)
            .setDescription(patreonConfig.DESCRIPTION)
            .setURL(patreonConfig.URL)
            .setThumbnail(patreonConfig.THUMBNAIL)
        );
    }
}

module.exports = SupportCommand;