import UserGroups = require('../../client/UserGroups');
import { Format } from '../../modules/discord/DiscordFormatter';
import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';
import { BaseGuildTextChannel, ThreadChannel } from 'discord.js';

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

    async run({ message, client }: CommandMessageContext, { channel }: { channel: BaseGuildTextChannel | ThreadChannel }): Promise<void> {
        await client.master.registry.guilds.setSpamChannelAsync(channel);

        await client.sendEmbedSuccess(message.channel, `Successfully made ${Format.guildChannel(channel, '#name (`#id`)')} a spam channel.`);
    }
}

export = AddSpamChannelCommand;
