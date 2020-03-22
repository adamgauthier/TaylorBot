'use strict';

const moment = require('moment');

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class UserRegistry {
    constructor(database, redis) {
        this.database = database;
        this.redis = redis;
    }

    key(userId) {
        return `ignore-until:user:${userId}`;
    }

    async getIgnoredUntil(user) {
        const key = this.key(user.id);
        const cachedIgnoreUntil = await this.redis.get(key);

        if (cachedIgnoreUntil === null) {
            const { ignore_until, was_inserted, username_changed, previous_username } = await this.database.users.insertOrGetUserIgnoreUntil(user);

            if (was_inserted) {
                Log.verbose(`Added new user ${Format.user(user)}.`);
                await this.database.usernames.addNewUsernameAsync(user);
            }
            else if (username_changed) {
                await this.database.usernames.addNewUsernameAsync(user);
                Log.verbose(`Added new username for ${Format.user(user)}, previously was '${previous_username}'.`);
            }

            const ignoreUntil = moment(ignore_until);
            await this.redis.setExpire(
                key,
                1 * 60 * 60,
                ignoreUntil.format('x')
            );
            return ignoreUntil;
        }

        return moment(cachedIgnoreUntil, 'x');
    }

    async ignoreUser(user, ignoreUntil) {
        await this.database.users.ignore(user, ignoreUntil);
        await this.redis.delete(this.key(user.id));
    }
}

module.exports = UserRegistry;
