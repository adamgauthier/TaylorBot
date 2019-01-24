'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

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
            lastCommand: 0,
            lastAnswered: 1,
            ignoreUntil: databaseUser.ignore_until.getTime()
        });
    }

    async addUser(member, discoveredAt) {
        const { user } = member;
        if (this.has(user.id)) {
            throw new Error(`User ${Format.user(user)} was already cached when attempting to add.`);
        }

        Log.verbose(`Adding new user from member ${Format.member(member)}.`);

        const inserted = await this.database.users.add(member, discoveredAt);

        this.cacheUser(inserted);
    }

    async ignoreUser(user, ignoreUntil) {
        const cachedUser = this.get(user.id);

        await this.database.users.ignore(user, ignoreUntil);

        cachedUser.ignoreUntil = ignoreUntil.getTime();
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