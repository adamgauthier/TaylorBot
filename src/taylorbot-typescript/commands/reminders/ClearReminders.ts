import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ClearRemindersCommand extends Command {
    constructor() {
        super({
            name: 'clearreminders',
            aliases: ['clearreminder', 'cr'],
            group: 'Reminders ⏰',
            description: 'This command has been removed. Please use **/remind manage** instead.',
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

    async run({ message, client, author }: CommandMessageContext): Promise<void> {
        await client.sendEmbedError(
            message.channel,
            `This command has been removed. Please use **/remind manage** instead.`
        );
    }
}

export = ClearRemindersCommand;
