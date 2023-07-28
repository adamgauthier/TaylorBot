import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class RemoveSpamChannelCommand extends Command {
    constructor() {
        super({
            name: 'removespamchannel',
            aliases: ['rsc'],
            group: 'admin',
            description: 'This command has been removed. Please use </mod spam remove:838266590294048778> instead.',
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
            'This command has been removed.',
            'Please use </mod spam remove:838266590294048778> instead.'
        ].join('\n'));
    }
}

export = RemoveSpamChannelCommand;
