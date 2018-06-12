'use strict';

const { Paths } = require('globalobjects');

const Command = require(Paths.Command);

class ClearRemindersCommand extends Command {
    constructor() {
        super({
            name: 'clearreminders',
            aliases: ['clearreminder', 'cr'],
            group: 'reminders',
            description: 'Clears all your reminders.',
            examples: [`clearreminders`],

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