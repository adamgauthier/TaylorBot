'use strict';

const moment = require('moment');

class UserRegistry {
    constructor(database, redis) {
        this.database = database;
        this.redis = redis;
    }

    key(userId) {
        return `ignored-user:${userId}`;
    }

    async cacheIgnored() {
        const users = await this.database.users.getAllIgnored();
        await Promise.all(users.map(u => this.cacheIgnoredUser(u)));
    }

    getIgnoredUntil(userId) {
        return this.redis.get(this.key(userId));
    }

    async cacheIgnoredUser(databaseUser) {
        const secondsUntilNotIgnored = moment.utc(databaseUser.ignore_until, 'X').diff(moment.utc(), 'seconds');
        await this.redis.setExpire(
            this.key(databaseUser.user_id),
            secondsUntilNotIgnored,
            databaseUser.ignore_until.getTime()
        );
    }

    async ignoreUser(user, ignoreUntil) {
        await this.database.users.ignore(user, ignoreUntil);
        await this.cacheIgnoredUser({ user_id: user.id, ignore_until: ignoreUntil });
    }
}

module.exports = UserRegistry;
