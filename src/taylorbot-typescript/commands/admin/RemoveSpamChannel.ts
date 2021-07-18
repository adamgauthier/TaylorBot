import { NewsChannel, TextChannel } from 'discord.js';
import UserGroups = require('../../client/UserGroups');
import { Format } from '../../modules/discord/DiscordFormatter';
import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class RemoveSpamChannelCommand extends Command {
    constructor() {
        super({
            name: 'removespamchannel',
            aliases: ['rsc'],
            group: 'admin',
            description: 'Indicates the bot that it should stop considering a channel as spam.',
            minimumGroup: UserGroups.GuildManagers,
            examples: ['#spam', 'spam-and-bots'],
            guildOnly: true,

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like the bot to consider as non-spam?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { channel }: { channel: TextChannel | NewsChannel }): Promise<void> {
        await client.master.registry.guilds.removeSpamChannelAsync(channel);

        await client.sendEmbedSuccess(message.channel, `Successfully removed ${Format.guildChannel(channel, '#name (`#id`)')} as a spam channel.`);
    }
}

export = RemoveSpamChannelCommand;
