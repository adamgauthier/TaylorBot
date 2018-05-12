'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class UserRegistry extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async load() {
        const users = await this.database.users.getAll();
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

        let databaseUser = await this.database.users.get(user);
        if (!databaseUser) {
            databaseUser = await this.database.users.add(user);
            Log.verbose(`Added user ${Format.user(user)} to database.`);
        }

        if (this.has(databaseUser.user_id)) {
            Log.warn(`Adding user ${Format.user(user)}, already cached, overwriting with database user.`);
        }

        this.cacheUser(databaseUser);
    }

    updateLastCommand(user, lastCommand) {
        const cachedUser = this.get(user.id);

        if (!cachedUser)
            throw new Error(`Can't update lastCommand of user ${Format.user(user)} because it wasn't cached.`);
        
        cachedUser.lastCommand = lastCommand;
    }

    updateLastAnswered(user, lastAnswered) {
        const cacheUser = this.get(user.id);

        if (!cacheUser)
            throw new Error(`Can't update lastAnswered of user ${Format.user(user)} because it wasn't cached.`);
        
        cacheUser.lastAnswered = lastAnswered;
    }
}

module.exports = UserRegistry;