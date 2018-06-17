'use strict';

const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class UserUpdate extends EventHandler {
    constructor() {
        super('userUpdate');
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