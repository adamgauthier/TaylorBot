import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class EnableServerCommandCommand extends Command {
    constructor() {
        super({
            name: 'enableservercommand',
            aliases: ['enableguildcommand', 'egc', 'esc'],
            group: 'framework',
            description: 'This command is obsolete and will be removed in a future version. Please use **/command server-enable** instead.',
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

    async run({ message, client }: CommandMessageContext, { args }: { args: string }): Promise<void> {
        await client.sendEmbedError(message.channel, [
            `This command is obsolete and will be removed in a future version.`,
            `Please use **/command server-enable** instead.`
        ].join('\n'));
    }
}

export = EnableServerCommandCommand;
