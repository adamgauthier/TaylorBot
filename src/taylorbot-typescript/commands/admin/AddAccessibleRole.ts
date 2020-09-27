import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class GetRoleCommand extends Command {
    constructor() {
        super({
            name: 'addaccessiblerole',
            aliases: ['aar'],
            group: 'admin',
            description: 'This command is obsolete and will be removed in a future version. Please use the `roles add` command instead.',
            examples: [''],
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
            `This command is obsolete and will be removed in a future version.`,
            `Please use \`${messageContext.prefix}roles add ${args}\` instead.`
        ].join('\n'));
    }
}

export = GetRoleCommand;
