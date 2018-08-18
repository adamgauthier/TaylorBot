'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class ReminderRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.users.reminders.find();
        }
        catch (e) {
            Log.error(`Getting all reminders: ${e}`);
            throw e;
        }
    }

    async fromUser(user) {
        try {
            return await this._db.users.reminders.find({
                'user_id': user.id
            });
        }
        catch (e) {
            Log.error(`Getting reminder from user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async add(user, remindAt, reminderText) {
        try {
            return await this._db.users.reminders.insert({
                'user_id': user.id,
                'created_at': Date.now(),
                'remind_at': remindAt,
                'reminder_text': reminderText
            });
        }
        catch (e) {
            Log.error(`Adding reminder for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async remove(reminderId) {
        try {
            return await this._db.users.reminders.destroy(reminderId);
        }
        catch (e) {
            Log.error(`Removing reminder ${reminderId}: ${e}`);
            throw e;
        }
    }

    async removeFrom(user) {
        try {
            return await this._db.users.reminders.destroy({
                'user_id': user.id
            });
        }
        catch (e) {
            Log.error(`Removing reminders for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = ReminderRepository;