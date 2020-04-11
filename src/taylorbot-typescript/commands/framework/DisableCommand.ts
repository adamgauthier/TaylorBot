import UserGroups = require('../../client/UserGroups.js');
import Command = require('../Command.js');
import CommandError = require('../CommandError.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { CachedCommand } from '../../client/registry/CachedCommand';

class DisableCommandCommand extends Command {
    constructor() {
        super({
            name: 'disablecommand',
            aliases: ['dc'],
            group: 'framework',
            description: 'Disables a command globally.',
            minimumGroup: UserGroups.Master,
            examples: ['avatar', 'uinfo'],

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to disable?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { command }: { command: CachedCommand }): Promise<void> {
        const isDisabled = await client.master.registry.commands.insertOrGetIsCommandDisabled(command);

        if (isDisabled) {
            throw new CommandError(`Command '${command.name}' is already disabled.`);
        }

        if (command.command.minimumGroup === UserGroups.Master) {
            throw new CommandError(`Can't disable '${command.name}' because it's a Master command.`);
        }

        if (command.command.group === 'framework') {
            throw new CommandError(`Can't disable '${command.name}' because it's a framework command.`);
        }

        await command.disableCommand();
        await client.sendEmbedSuccess(message.channel, `Successfully disabled '${command.name}' globally.`);
    }
}

export = DisableCommandCommand;
