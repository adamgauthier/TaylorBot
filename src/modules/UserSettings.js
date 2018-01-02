'use strict';

const path = require('path');

const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

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
            'lastCommand': databaseUser.last_command,
            'lastAnswered': databaseUser.last_answered,
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
}

module.exports = UserSettings;