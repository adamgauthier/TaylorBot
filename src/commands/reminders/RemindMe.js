'use strict';

const { Paths } = require('globalobjects');

const Command = require(Paths.Command);
const CommandError = require(Paths.CommandError);
const TimeUtil = require(Paths.TimeUtil);

const REMINDERS_LIMIT = 3;

class RemindMeCommand extends Command {
    constructor() {
        super({
            name: 'remindme',
            aliases: ['remind', 'reminder'],
            group: 'reminders',
            description: 'Sets a reminder for the future.',
            examples: [`remindme thursday 'listening party'`, `reminder dec 13th 2018 "taylor's birthday"`],

            args: [
                {
                    key: 'time',
                    label: 'when',
                    type: 'future-time',
                    prompt: 'When would you like to be reminded?'
                },
                {
                    key: 'reminder',
                    label: 'message',
                    type: 'quoted-multiline-text',
                    prompt: 'What would you like to be reminded about?'
                }
            ]
        });
    }

    async run({ message, client }, { time, reminder }) {
        const { author, channel } = message;
        const { reminders } = client.master.database;

        if (time.diff(Date.now(), 'years', true) > 1) {
            throw new CommandError(`You can't be reminded more than a year in the future!`);
        }

        const { length: reminderCount } = await reminders.fromUser(author);

        if (reminderCount >= REMINDERS_LIMIT) {
            throw new CommandError(`You can't have more than ${REMINDERS_LIMIT} reminders active at the same time!`);
        }

        const unixTime = time.valueOf();
        await reminders.add(author, unixTime, reminder);

        return client.sendEmbedSuccess(channel, `Okay, I will remind you on \`${TimeUtil.formatLog(unixTime)}\` about '${reminder}'. 😊`);
    }
}

module.exports = RemindMeCommand;