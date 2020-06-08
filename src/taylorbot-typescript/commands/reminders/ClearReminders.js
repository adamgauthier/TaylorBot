'use strict';

const Command = require('../Command.js');

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

    async run({ message, client }) {
        const { reminders } = client.master.database;

        const removed = await reminders.removeFrom(message.author);

        return client.sendEmbedSuccess(message.channel, `Successfully cleared \`${removed.length}\` reminders. 😊`);
    }
}

module.exports = ClearRemindersCommand;
