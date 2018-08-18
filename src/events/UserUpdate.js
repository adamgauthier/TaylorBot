'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class UserUpdate extends EventHandler {
    constructor() {
        super(Events.USER_UPDATE);
    }

    async handler({ master }, oldUser, newUser) {
        if (oldUser.username !== newUser.username) {
            const changedAt = Date.now();
            await master.database.usernames.add(newUser, changedAt);
            Log.info(`Added new username for ${Format.user(newUser)}. Old username was ${oldUser.username}.`);
        }
    }
}

module.exports = UserUpdate;