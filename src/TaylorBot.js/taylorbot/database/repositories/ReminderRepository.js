'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class ReminderRepository {
    constructor(db) {
        this._db = db;
    }

    async fromUser(user) {
        try {
            return await this._db.any(
                'SELECT * FROM users.reminders WHERE user_id = $[user_id];',
                {
                    'user_id': user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting reminder from user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async add(user, remindAt, reminderText) {
        try {
            return await this._db.none(
                `INSERT INTO users.reminders (user_id, remind_at, reminder_text)
                VALUES ($[user_id], $[remind_at], $[reminder_text]);`,
                {
                    'user_id': user.id,
                    'remind_at': remindAt,
                    'reminder_text': reminderText
                }
            );
        }
        catch (e) {
            Log.error(`Adding reminder for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async removeFrom(user) {
        try {
            return await this._db.any(
                'DELETE FROM users.reminders WHERE user_id = $[user_id] RETURNING *;',
                { user_id: user.id }
            );
        }
        catch (e) {
            Log.error(`Removing reminders for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = ReminderRepository;