import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class RemoveSpamChannelCommand extends Command {
    constructor() {
        super({
            name: 'removespamchannel',
            aliases: ['rsc'],
            group: 'admin',
            description: 'This command has been removed. Please use **/mod spam remove** instead.',
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
            'Please use **/mod spam remove** instead.'
        ].join('\n'));
    }
}

export = RemoveSpamChannelCommand;
