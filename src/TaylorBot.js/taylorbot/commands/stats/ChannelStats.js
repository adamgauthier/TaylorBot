'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');

class ChannelStatsCommand extends Command {
    constructor() {
        super({
            name: 'channelstats',
            aliases: ['cstats'],
            group: 'stats',
            description: 'Gets stored stats about a text channel.',
            examples: ['', '#general'],

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like to see the stats of?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { channel }) {
        const textChannel = await client.master.database.textChannels.get(channel);

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(channel.guild)
            .setTitle(channel.name)
            .addField('Messages', textChannel.message_count, true)
            .addField('Member Log', textChannel.is_member_log ? '✅' : '🚫', true)
            .addField('Message Log', textChannel.is_message_log ? '✅' : '🚫', true)
            .addField('Spam', textChannel.is_spam ? '✅' : '🚫', true);

        return client.sendEmbed(message.channel, embed);
    }
}

module.exports = ChannelStatsCommand;
