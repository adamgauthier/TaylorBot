'use strict';

class UserRegistry extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async load() {
        const users = await this.database.users.getAllIgnored();
        users.forEach(u => this.cacheUser(u));
    }

    cacheUser(databaseUser) {
        this.set(databaseUser.user_id, {
            ignoreUntil: databaseUser.ignore_until.getTime()
        });
    }

    getOrAdd(user) {
        const cachedUser = this.get(user.id);
        if (!cachedUser) {
            this.cacheUser({ user_id: user.id, ignore_until: new Date() });
            return this.get(user.id);
        }
        return cachedUser;
    }

    async ignoreUser(user, ignoreUntil) {
        const cachedUser = this.getOrAdd(user);

        await this.database.users.ignore(user, ignoreUntil);

        cachedUser.ignoreUntil = ignoreUntil.getTime();
    }
}

module.exports = UserRegistry;
