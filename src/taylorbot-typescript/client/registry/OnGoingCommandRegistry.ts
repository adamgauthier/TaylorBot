import { RedisDriver } from '../../caching/RedisDriver';
import { User } from 'discord.js';

const SECONDS_EXPIRE = 10;

export class OnGoingCommandRegistry {
    #redis: RedisDriver;

    constructor(redis: RedisDriver) {
        this.#redis = redis;
    }

    key(user: User): string {
        return `ongoing-commands:user:${user.id}`;
    }

    async addOngoingCommandAsync(user: User): Promise<void> {
        const key = this.key(user);
        await this.#redis.increment(key);
        await this.#redis.expire(key, SECONDS_EXPIRE);
    }

    async removeOngoingCommandAsync(user: User): Promise<void> {
        const key = this.key(user);
        await this.#redis.decrement(key);
    }

    async hasAnyOngoingCommandAsync(user: User): Promise<boolean> {
        const key = this.key(user);
        const ongoingCommands = await this.#redis.get(key);

        return ongoingCommands !== null && Number.parseInt(ongoingCommands) > 0;
    }
}
