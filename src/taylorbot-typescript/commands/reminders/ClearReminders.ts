import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ClearRemindersCommand extends Command {
    constructor() {
        super({
            name: 'clearreminders',
            aliases: ['clearreminder', 'cr'],
            group: 'Reminders ⏰',
            description: 'Clears all your reminders.',
            examples: [''],

            args: []
        });
    }

    async run({ message, client, author }: CommandMessageContext): Promise<void> {
        const { reminders } = client.master.database;

        const removed = await reminders.removeFrom(author);

        await client.sendEmbedSuccess(message.channel, `Successfully cleared \`${removed.length}\` reminders. 😊`);
    }
}

export = ClearRemindersCommand;
