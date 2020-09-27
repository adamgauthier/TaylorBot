import UserGroups = require('../../client/UserGroups');
import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { CommandMessageContext } from '../CommandMessageContext';
import { DatabaseCommand } from '../../database/repositories/CommandRepository';

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
                    key: 'databaseCommand',
                    label: 'command',
                    type: 'database-command',
                    prompt: 'What command would you like to disable?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { databaseCommand }: { databaseCommand: DatabaseCommand }): Promise<void> {
        if (!databaseCommand.enabled) {
            throw new CommandError(`Command \`${databaseCommand.name}\` is already disabled.`);
        }

        if (databaseCommand.module_name.toLowerCase() === 'framework') {
            throw new CommandError(`Can't disable \`${databaseCommand.name}\` because it's a framework command.`);
        }

        await client.master.registry.commands.setGlobalEnabled(databaseCommand.name, false);
        await client.sendEmbedSuccess(message.channel, `Successfully disabled \`${databaseCommand.name}\` globally.`);
    }
}

export = DisableCommandCommand;
