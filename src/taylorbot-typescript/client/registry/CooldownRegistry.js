'use strict';

const moment = require('moment');

class CooldownRegistry {
    constructor(redis) {
        this.redis = redis;
    }

    key(user, command) {
        const unixDay = moment.utc().startOf('day').format('X');
        return `cooldown:${unixDay}:user:${user.id}:command:${command.name}`;
    }

    async getDailyUseCount(user, command) {
        const result = await this.redis.get(
            this.key(user, command)
        );
        return result || 0;
    }

    addDailyUseCount(user, command) {
        const secondsUntilEndOfDay = moment.utc().endOf('day').diff(moment.utc(), 'seconds');
        const key = this.key(user, command);

        const multi = this.redis.multi();

        multi.incr(key);
        multi.expire(key, secondsUntilEndOfDay);

        return multi.execute();
    }
}

module.exports = CooldownRegistry;