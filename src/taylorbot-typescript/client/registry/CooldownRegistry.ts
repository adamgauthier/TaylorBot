import moment = require('moment');
import { RedisDriver } from '../../caching/RedisDriver';
import { User } from 'discord.js';
import { CachedCommand } from './CachedCommand';

export class CooldownRegistry {
    #redis: RedisDriver;

    constructor(redis: RedisDriver) {
        this.#redis = redis;
    }

    key(user: User, command: CachedCommand): string {
        const unixDay = moment.utc().startOf('day').format('X');
        return `cooldown:${unixDay}:user:${user.id}:command:${command.name}`;
    }

    async getDailyUseCount(user: User, command: CachedCommand): Promise<number> {
        const result = await this.#redis.get(
            this.key(user, command)
        );

        if (result === null)
            return 0;

        return Number.parseInt(result);
    }

    async addDailyUseCount(user: User, command: CachedCommand): Promise<void> {
        const secondsUntilEndOfDay = moment.utc().endOf('day').diff(moment.utc(), 'seconds');
        const key = this.key(user, command);

        const multi = this.#redis.multi();

        multi.incr(key);
        multi.expire(key, secondsUntilEndOfDay);

        await multi.execute();
    }
}
