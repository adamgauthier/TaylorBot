import { TextChannel } from 'discord.js';
import UserGroups = require('../../client/UserGroups');
import { Format } from '../../modules/discord/DiscordFormatter';
import { Command } from '../Command';
import { CommandError } from '../CommandError';
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

    async run({ message, client }: CommandMessageContext, { channel }: { channel: TextChannel }): Promise<void> {
        const { database } = client.master;
        const textChannel = await database.textChannels.get(channel);

        if (!textChannel) {
            await database.textChannels.upsertSpamChannel(channel, false);
        }
        else if (!textChannel.is_spam) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is not a spam channel.`);
        }
        else {
            await database.textChannels.removeSpam(channel);
        }

        await client.sendEmbedSuccess(message.channel, `Successfully removed ${Format.guildChannel(channel, '#name (`#id`)')} as a spam channel.`);
    }
}

export = RemoveSpamChannelCommand;
