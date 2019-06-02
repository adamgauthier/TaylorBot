'use strict';

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

    async ignoreUser(user, ignoreUntil) {
        const cachedUser = this.get(user.id);

        await this.database.users.ignore(user, ignoreUntil);

        cachedUser.ignoreUntil = ignoreUntil.getTime();
    }
}

module.exports = UserRegistry;