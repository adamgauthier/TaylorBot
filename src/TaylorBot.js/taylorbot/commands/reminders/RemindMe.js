'use strict';

const moment = require('moment');

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const TimeUtil = require('../../modules/TimeUtil.js');

const REMINDERS_LIMIT = 3;

class RemindMeCommand extends Command {
    constructor() {
        super({
            name: 'remindme',
            aliases: ['remind', 'reminder'],
            group: 'reminders',
            description: 'Sets a reminder for the future.',
            examples: [`1 hour watch movie`, `10 minutes pizza party`],

            args: [
                {
                    key: 'event',
                    label: 'event',
                    type: 'future-event',
                    prompt: 'When and what would you like to be reminded about?'
                }
            ]
        });
    }

    async run({ message, client }, { event }) {
        const { author, channel } = message;
        const { reminders } = client.master.database;

        const remindAt = moment(event.startDate);
        const remindAbout = event.eventTitle;

        if (remindAt.diff(Date.now(), 'years', true) > 1) {
            throw new CommandError(`You can't be reminded more than a year in the future!`);
        }

        const { length: reminderCount } = await reminders.fromUser(author);

        if (reminderCount >= REMINDERS_LIMIT) {
            throw new CommandError(`You can't have more than ${REMINDERS_LIMIT} reminders active at the same time!`);
        }

        await reminders.add(author, remindAt.toDate(), remindAbout);

        return client.sendEmbedSuccess(channel, `Okay, I will remind you on \`${TimeUtil.formatLog(remindAt.valueOf())}\` about '${remindAbout}'. 😊`);
    }
}

module.exports = RemindMeCommand;