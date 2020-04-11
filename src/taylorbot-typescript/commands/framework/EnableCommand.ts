import UserGroups = require('../../client/UserGroups.js');
import Command = require('../Command.js');
import CommandError = require('../CommandError.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { CachedCommand } from '../../client/registry/CachedCommand';

class EnableCommandCommand extends Command {
    constructor() {
        super({
            name: 'enablecommand',
            aliases: ['ec'],
            group: 'framework',
            description: 'Enables a disabled command globally.',
            minimumGroup: UserGroups.Master,
            examples: ['avatar', 'uinfo'],

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to enable?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { command }: { command: CachedCommand }): Promise<void> {
        const isDisabled = await client.master.registry.commands.insertOrGetIsCommandDisabled(command);

        if (!isDisabled) {
            throw new CommandError(`Command '${command.name}' is already enabled.`);
        }

        await command.enableCommand();
        await client.sendEmbedSuccess(message.channel, `Successfully enabled '${command.name}' globally.`);
    }
}

export = EnableCommandCommand;
