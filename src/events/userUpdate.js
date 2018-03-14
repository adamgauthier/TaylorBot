'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const database = require(GlobalPaths.databaseDriver);

class UserUpdate extends EventHandler {
    constructor() {
        super(async (oldUser, newUser) => {
            if (oldUser.username !== newUser.username) {
                const changedAt = new Date().getTime();
                await database.addUsername(newUser, changedAt);
                Log.info(`Added new username for ${Format.user(newUser)}. Old username was ${oldUser.username}.`);
            }
        });
    }
}

module.exports = new UserUpdate();