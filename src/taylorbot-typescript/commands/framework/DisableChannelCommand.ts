import Command = require('../Command.js');
import CommandError = require('../CommandError.js');
import UserGroups = require('../../client/UserGroups.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { TextChannel } from 'discord.js';

class DisableChannelCommandCommand extends Command {
    constructor() {
        super({
            name: 'disablechannelcommand',
            aliases: ['dcc'],
            group: 'framework',
            description: 'Disables a command in a channel.',
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
                    prompt: 'What channel would you like to disable the command in?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { command, channel }: { command: CachedCommand; channel: TextChannel }): Promise<void> {
        if (command.command.minimumGroup === UserGroups.Master) {
            throw new CommandError(`Can't disable \`${command.name}\` because it's a Master command.`);
        }

        if (command.command.group === 'framework') {
            throw new CommandError(`Can't disable \`${command.name}\` because it's a framework command.`);
        }

        await client.master.registry.channelCommands.disableCommandInChannel(channel, command);

        await client.sendEmbedSuccess(message.channel, `Successfully disabled \`${command.name}\` in ${channel}.`);
    }
}

export = DisableChannelCommandCommand;
