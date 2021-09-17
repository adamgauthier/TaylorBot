import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class DisableChannelCommandCommand extends Command {
    constructor() {
        super({
            name: 'disablechannelcommand',
            aliases: ['dcc'],
            group: 'framework',
            description: 'This command is obsolete and will be removed in a future version. Please use **/command channel-disable** instead.',
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
            `Please use **/command channel-disable** instead.`
        ].join('\n'));
    }
}

export = DisableChannelCommandCommand;
