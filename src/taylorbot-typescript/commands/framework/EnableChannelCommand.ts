import Command = require('../Command.js');
import UserGroups = require('../../client/UserGroups.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { TextChannel } from 'discord.js';

class EnableChannelCommandCommand extends Command {
    constructor() {
        super({
            name: 'enablechannelcommand',
            aliases: ['ecc'],
            group: 'framework',
            description: 'Enables a disabled command in a channel.',
            minimumGroup: UserGroups.Moderators,
            examples: ['roll general', 'gamble'],
            guildOnly: true,

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to enable?'
                },
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like to enable the command in?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { command, channel }: { command: CachedCommand; channel: TextChannel }): Promise<void> {
        await client.master.registry.channelCommands.enableCommandInChannel(channel, command);

        await client.sendEmbedSuccess(message.channel, `Successfully enabled \`${command.name}\` in ${channel}.`);
    }
}

export = EnableChannelCommandCommand;
