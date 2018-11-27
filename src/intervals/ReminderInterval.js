'use strict';

const moment = require('moment');

const Interval = require('../structures/Interval.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');
const EmbedUtil = require('../modules/EmbedUtil.js');

const intervalTime = 120000;

class ReminderInterval extends Interval {
    constructor() {
        super(intervalTime);
    }

    async interval(client) {
        const { master } = client;
        const { database } = master;
        const reminders = await database.reminders.getAll();

        for (const reminder of reminders) {
            if (moment().isAfter(moment.utc(reminder.remind_at, 'x', true))) {
                const user = master.resolveUser(reminder.user_id);

                if (user) {
                    try {
                        await master.sendEmbed(user,
                            EmbedUtil.success(reminder.reminder_text)
                                .setAuthor('Reminder')
                                .setTimestamp(moment.utc(reminder.created_at, 'x', true).toDate())
                        );
                        Log.info(`Reminded user ${Format.user(user)} about '${reminder.reminder_text}'.`);
                    }
                    catch (e) {
                        Log.error(`Could not remind user ${Format.user(user)} about '${reminder.reminder_text}': ${e}`);
                    }
                    finally {
                        await database.reminders.remove(reminder.reminder_id);
                    }
                }
                else {
                    Log.warn(`Could not resolve user id ${reminder.user_id} when trying to remind from reminder '${reminder.reminder_id}'.`);
                }
            }
        }
    }
}

module.exports = ReminderInterval;