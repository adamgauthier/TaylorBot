'use strict';

const TaypointAmount = require('../../modules/points/TaypointAmount.js');

class HeistRegistry {
    constructor(redis) {
        this.redis = redis;
    }

    key(guild) {
        return `heist:guild:${guild.id}`;
    }

    async enterHeist(user, guild, amount, heistDelayMinutes) {
        const key = this.key(guild);

        const [exists, wasSet] = await this.redis.eval(
            `local exists = redis.call('exists', KEYS[1])
            local wasSet = redis.call('hset', KEYS[1], ARGV[1], ARGV[2])
            if exists == 0 then
                redis.call('expire', KEYS[1], ARGV[3])
            end
            return {exists, wasSet}`,
            1,
            key,
            user.id,
            JSON.stringify(amount),
            (heistDelayMinutes + 1) * 60
        );

        return {
            created: !exists,
            updated: !wasSet
        };
    }

    async completeHeist(guild) {
        const key = this.key(guild);
        const multi = this.redis.multi();

        multi.hgetall(key);
        multi.del(key);

        const [keys] = await multi.execute();

        return Object.entries(keys)
            .map(([userId, amount]) => ({ userId, amount: new TaypointAmount(JSON.parse(amount)) }));
    }
}

module.exports = HeistRegistry;