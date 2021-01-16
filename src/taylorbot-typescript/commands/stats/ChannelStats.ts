import { TextChannel } from 'discord.js';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ChannelStatsCommand extends Command {
    constructor() {
        super({
            name: 'channelstats',
            aliases: ['cstats'],
            group: 'Stats 📊',
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

    async run({ message, client }: CommandMessageContext, { channel }: { channel: TextChannel }): Promise<void> {
        const textChannel = (await client.master.database.textChannels.get(channel))!;

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(channel.guild)
            .setTitle(channel.name)
            .addField('Messages', `~${textChannel.message_count}`, true)
            .addField('Member Log', textChannel.is_member_log ? '✅' : '🚫', true)
            .addField('Message Log', textChannel.is_message_log ? '✅' : '🚫', true)
            .addField('Spam', textChannel.is_spam ? '✅' : '🚫', true);

        await client.sendEmbed(message.channel, embed);
    }
}

export = ChannelStatsCommand;
