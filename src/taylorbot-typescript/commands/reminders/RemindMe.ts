import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class RemindMeCommand extends Command {
    constructor() {
        super({
            name: 'remindme',
            aliases: ['reminder'],
            group: 'Reminders ⏰',
            description: 'This command has been removed. Please use </remind add:861754955728027678> instead.',
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
        await client.sendEmbedError(
            message.channel,
            `This command has been removed. Please use </remind add:861754955728027678> instead.`
        );
    }
}

export = RemindMeCommand;
