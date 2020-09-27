import { Command } from '../Command';
import { CommandError } from '../CommandError';
import UserGroups = require('../../client/UserGroups');
import { CommandMessageContext } from '../CommandMessageContext';
import { TextChannel } from 'discord.js';
import { DatabaseCommand } from '../../database/repositories/CommandRepository';

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
                    key: 'databaseCommand',
                    label: 'command',
                    type: 'database-command',
                    prompt: 'What command would you like to disable?'
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

    async run({ message, client }: CommandMessageContext, { databaseCommand, channel }: { databaseCommand: DatabaseCommand; channel: TextChannel }): Promise<void> {
        if (databaseCommand.module_name.toLowerCase() === 'framework') {
            throw new CommandError(`Can't disable \`${databaseCommand.name}\` because it's a framework command.`);
        }

        await client.master.registry.channelCommands.disableCommandInChannel(channel, databaseCommand);

        await client.sendEmbedSuccess(message.channel, `Successfully disabled \`${databaseCommand.name}\` in ${channel}.`);
    }
}

export = DisableChannelCommandCommand;
