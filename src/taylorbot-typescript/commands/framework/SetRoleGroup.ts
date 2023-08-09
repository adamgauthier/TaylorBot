import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class SetRoleGroupCommand extends Command {
    constructor() {
        super({
            name: 'setrolegroup',
            aliases: ['srg'],
            group: 'framework',
            description: 'This command has been removed. Permissions in TaylorBot now rely on Discord permissions directly!',
            examples: [''],

            args: [
                {
                    key: 'args',
                    label: 'args',
                    type: 'any-text',
                    prompt: 'What arguments would you like to use?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext): Promise<void> {
        await client.sendEmbedError(message.channel, [
            `This command has been removed.`,
            `Permissions in TaylorBot now rely on Discord permissions directly!`
        ].join('\n'));
    }
}

export = SetRoleGroupCommand;
