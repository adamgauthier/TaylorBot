import TaypointAmount = require('../../modules/points/TaypointAmount.js');
import { RedisDriver } from '../../caching/RedisDriver.js';
import { Guild, User } from 'discord.js';

export class HeistRegistry {
    #redis: RedisDriver;

    constructor(redis: RedisDriver) {
        this.#redis = redis;
    }

    key(guild: Guild): string {
        return `heist:guild:${guild.id}`;
    }

    async enterHeist(user: User, guild: Guild, amount: TaypointAmount, heistDelayMinutes: number): Promise<{ created: boolean; updated: boolean }> {
        const key = this.key(guild);

        const [exists, wasSet] = await this.#redis.eval(
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

    async completeHeist(guild: Guild): Promise<{ userId: string; amount: TaypointAmount }[]> {
        const key = this.key(guild);
        const multi = this.#redis.multi();

        multi.hgetall(key);
        multi.del(key);

        const [keys]: { [key: string]: string }[] = await multi.execute();

        return Object.entries(keys)
            .map(([userId, amount]) => ({ userId, amount: new TaypointAmount(JSON.parse(amount)) }));
    }
}
