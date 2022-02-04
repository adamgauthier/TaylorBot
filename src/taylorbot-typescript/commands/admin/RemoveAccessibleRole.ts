import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class RemoveAccessibleRoleCommand extends Command {
    constructor() {
        super({
            name: 'removeaccessiblerole',
            aliases: ['rar'],
            group: 'admin',
            description: 'This command has been removed. Please use the `roles remove` command instead.',
            examples: ['', ''],
            guildOnly: true,

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
            `Please use \`${messageContext.prefix}roles remove ${args}\` instead.`
        ].join('\n'));
    }
}

export = RemoveAccessibleRoleCommand;
