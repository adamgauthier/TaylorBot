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
            ignoreUntil: databaseUser.ignore_until.getTime()
        });
    }

    async addUser(member) {
        const { user } = member;
        if (this.has(user.id)) {
            throw new Error(`User ${Format.user(user)} was already cached when attempting to add.`);
        }

        Log.verbose(`Adding new user from member ${Format.member(member)}.`);

        const inserted = await this.database.users.add(member);

        if (inserted)
            this.cacheUser(inserted);
        else
            Log.warn(`Adding new user from member ${Format.member(member)}, user was already inserted.`);
    }

    async ignoreUser(user, ignoreUntil) {
        const cachedUser = this.get(user.id);

        await this.database.users.ignore(user, ignoreUntil);

        cachedUser.ignoreUntil = ignoreUntil.getTime();
    }
}

module.exports = UserRegistry;