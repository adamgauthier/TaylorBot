import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ImgurCommand extends Command {
    constructor() {
        super({
            name: 'imgur',
            group: 'Media 📷',
            description: 'This command has been removed. Please use **/imgur** instead.',
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
            'Please use **/imgur** instead.'
        ].join('\n'));
    }
}

export = ImgurCommand;
