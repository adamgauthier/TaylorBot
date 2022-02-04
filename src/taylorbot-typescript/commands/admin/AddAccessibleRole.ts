import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class AddAccessibleRoleCommand extends Command {
    constructor() {
        super({
            name: 'addaccessiblerole',
            aliases: ['aar'],
            group: 'admin',
            description: 'This command has been removed. Please use the `roles add` command instead.',
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
            `Please use \`${messageContext.prefix}roles add ${args}\` instead.`
        ].join('\n'));
    }
}

export = AddAccessibleRoleCommand;
