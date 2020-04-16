import UserGroups = require('../../client/UserGroups.js');
import Command = require('../Command.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { DatabaseCommand } from '../../database/repositories/CommandRepository';
import CommandError = require('../CommandError.js');

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
                    key: 'databaseCommand',
                    label: 'command',
                    type: 'database-command',
                    prompt: 'What command would you like to enable?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { databaseCommand }: { databaseCommand: DatabaseCommand }): Promise<void> {
        if (databaseCommand.enabled) {
            throw new CommandError(`Command \`${databaseCommand.name}\` is already enabled.`);
        }

        await client.master.registry.commands.setGlobalEnabled(databaseCommand.name, true);
        await client.sendEmbedSuccess(message.channel, `Successfully enabled \`${databaseCommand.name}\` globally.`);
    }
}

export = EnableCommandCommand;
