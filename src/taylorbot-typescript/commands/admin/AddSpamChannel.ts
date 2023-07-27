import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class AddSpamChannelCommand extends Command {
    constructor() {
        super({
            name: 'addspamchannel',
            aliases: ['asc'],
            group: 'admin',
            description: 'This command has been removed. Please use **/mod spam add** instead.',
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
            'Please use **/mod spam add** instead.'
        ].join('\n'));
    }
}

export = AddSpamChannelCommand;
