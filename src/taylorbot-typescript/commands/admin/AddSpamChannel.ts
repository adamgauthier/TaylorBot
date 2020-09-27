import UserGroups = require('../../client/UserGroups.js');
import Format = require('../../modules/DiscordFormatter.js');
import { Command } from '../Command';
import { CommandError } from '../../commands/CommandError';
import { CommandMessageContext } from '../CommandMessageContext';
import { TextChannel } from 'discord.js';

class AddSpamChannelCommand extends Command {
    constructor() {
        super({
            name: 'addspamchannel',
            aliases: ['asc'],
            group: 'admin',
            description: 'Indicates the bot that it should consider a channel as spam.',
            minimumGroup: UserGroups.GuildManagers,
            examples: ['#spam', 'spam-and-bots'],
            guildOnly: true,

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like the bot to consider as spam?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { channel }: { channel: TextChannel }): Promise<void> {
        const { database } = client.master;
        const textChannel = await database.textChannels.get(channel);

        if (!textChannel) {
            await database.textChannels.upsertSpamChannel(channel, true);
        }
        else if (textChannel.is_spam) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is already a spam channel.`);
        }
        else {
            await database.textChannels.setSpam(channel);
        }

        await client.sendEmbedSuccess(message.channel, `Successfully made ${Format.guildChannel(channel, '#name (`#id`)')} a spam channel.`);
    }
}

export = AddSpamChannelCommand;
