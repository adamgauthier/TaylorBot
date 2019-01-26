'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');

class ChannelInfoCommand extends Command {
    constructor() {
        super({
            name: 'channelinfo',
            aliases: ['cinfo'],
            group: 'discord',
            description: 'Gets information about a channel.',
            examples: ['', '#general'],

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like to see the info of?',
                    type: 'channel-or-current'
                }
            ]
        });
    }

    run({ message, client }, { channel }) {
        return client.sendEmbed(message.channel, DiscordEmbedFormatter.channel(channel));
    }
}

module.exports = ChannelInfoCommand;