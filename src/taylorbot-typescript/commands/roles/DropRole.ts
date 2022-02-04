import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class DropRoleCommand extends Command {
    constructor() {
        super({
            name: 'droprole',
            aliases: ['dr'],
            group: 'Roles 🆔',
            description: 'This command has been removed. Please use the `roles` command instead.',
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

    async run({ message, client, messageContext }: CommandMessageContext, { args }: { args: string }): Promise<void> {
        await client.sendEmbedError(message.channel, [
            `This command has been removed.`,
            `Please use \`${messageContext.prefix}role drop ${args}\` instead.`
        ].join('\n'));
    }
}

export = DropRoleCommand;
