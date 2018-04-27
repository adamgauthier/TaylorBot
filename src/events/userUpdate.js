'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class UserUpdate extends EventHandler {
    async handler(taylorbot, oldUser, newUser) {
        if (oldUser.username !== newUser.username) {
            const changedAt = new Date().getTime();
            await taylorbot.database.addUsername(newUser, changedAt);
            Log.info(`Added new username for ${Format.user(newUser)}. Old username was ${oldUser.username}.`);
        }
    }
}

module.exports = UserUpdate;