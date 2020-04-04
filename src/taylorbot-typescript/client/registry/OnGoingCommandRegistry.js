'use strict';

const SECONDS_EXPIRE = 10;

class OnGoingCommandRegistry {
    constructor(redis) {
        this.redis = redis;
    }

    key(user) {
        return `ongoing-commands:user:${user.id}`;
    }

    async addOngoingCommandAsync(user) {
        const key = this.key(user);
        await this.redis.increment(key);
        await this.redis.expire(key, SECONDS_EXPIRE);
    }

    async removeOngoingCommandAsync(user) {
        const key = this.key(user);
        await this.redis.decrement(key);
    }

    async hasAnyOngoingCommandAsync(user) {
        const key = this.key(user);
        const ongoingCommands = await this.redis.get(key);

        return ongoingCommands !== null && Number.parseInt(ongoingCommands) > 0;
    }
}

module.exports = OnGoingCommandRegistry;
