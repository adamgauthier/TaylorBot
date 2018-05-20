'use strict';

const { Paths } = require('globalobjects');

const DiscordEmbedFormatter = require(Paths.DiscordEmbedFormatter);
const Command = require(Paths.Command);

class ChannelInfoCommand extends Command {
    constructor() {
        super({
            name: 'channelinfo',
            aliases: ['cinfo'],
            group: 'info',
            memberName: 'channelinfo',
            description: 'Gets information about a channel.',
            examples: ['channelinfo', 'cinfo'],
            argsPromptLimit: 0,

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like to see the info of?',
                    type: 'channel-or-current',
                    error: 'Could not find channel'
                }
            ]
        });
    }

    run({ message, client }, { channel }) {
        return client.sendEmbed(message.channel, DiscordEmbedFormatter.channel(channel));
    }
}

module.exports = ChannelInfoCommand;