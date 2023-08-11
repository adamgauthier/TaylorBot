import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ProfileCommand extends Command {
    constructor() {
        super({
            name: 'profile',
            aliases: ['info', 'asl'],
            group: 'Stats 📊',
            description: 'This command has been removed. Please use </birthday age:1016938623880400907>, **/location show** instead.',
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
            `This command has been removed.`,
            `Please use </birthday age:1016938623880400907>, **/location show** instead.`
        ].join('\n'));
    }
}

export = ProfileCommand;
