'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class UserSettings extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async load() {
        const users = await this.database.getAllUsers();
        users.forEach(u => this.cacheUser(u));
    }

    cacheUser(databaseUser) {
        this.set(databaseUser.user_id, {
            'lastCommand': 0,
            'lastAnswered': 1,
            'ignoreUntil': databaseUser.ignore_until
        });
    }

    async addUser(user) {
        Log.verbose(`Adding user ${Format.user(user)}.`);

        let databaseUser = await this.database.getUser(user);
        if (!databaseUser) {
            databaseUser = await this.database.addUser(user);
            Log.verbose(`Added user ${Format.user(user)} to database.`);
        }

        if (this.has(databaseUser.user_id)) {
            Log.warn(`Adding user ${Format.user(user)}, already cached, overwriting with database user.`);
        }

        this.cacheUser(databaseUser);
    }

    updateLastCommand(user, lastCommand) {
        const settings = this.get(user.id);

        if (!settings)
            throw new Error(`Can't update lastCommand of user ${Format.user(user)} because it wasn't cached.`);
        
        settings.lastCommand = lastCommand;
        this.set(user.id, settings);
    }

    updateLastAnswered(user, lastAnswered) {
        const settings = this.get(user.id);

        if (!settings)
            throw new Error(`Can't update lastAnswered of user ${Format.user(user)} because it wasn't cached.`);
        
        settings.lastAnswered = lastAnswered;
        this.set(user.id, settings);
    }
}

module.exports = UserSettings;