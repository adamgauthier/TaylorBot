import { promisify } from 'util';
import redis = require('redis');

export class RedisDriver {
    readonly #redisClient: redis.RedisClient;
    readonly get: (key: string) => Promise<string>;
    readonly set: (key: string, value: string) => Promise<unknown>;
    readonly setExpire: (key: string, seconds: number, value: string) => Promise<string>;
    readonly eval: (script: string, ...args: any[]) => Promise<any>;
    readonly delete: (key: string) => Promise<number>;
    readonly exists: () => Promise<number>;
    readonly setAdd: () => Promise<number>;
    readonly setRemove: () => Promise<number>;
    readonly setIsMember: (key: string, member: string) => Promise<number>;
    readonly hashGet: (key: string, field: string) => Promise<string>;
    readonly hashSet: (key: string, field: string, value: string) => Promise<number>;
    readonly expire: (key: string, seconds: number) => Promise<number>;
    readonly increment: (key: string) => Promise<number>;
    readonly decrement: (key: string) => Promise<number>;

    constructor(host: string, port: number, password: string) {
        this.#redisClient = redis.createClient({
            host,
            port,
            password
        });

        this.get = promisify(this.#redisClient.get).bind(this.#redisClient);
        this.set = promisify(this.#redisClient.set).bind(this.#redisClient);
        this.setExpire = promisify(this.#redisClient.setex).bind(this.#redisClient);

        this.eval = promisify(this.#redisClient.eval).bind(this.#redisClient);
        this.delete = promisify(this.#redisClient.del).bind(this.#redisClient);
        this.exists = promisify(this.#redisClient.exists).bind(this.#redisClient);

        this.setAdd = promisify(this.#redisClient.sadd).bind(this.#redisClient);
        this.setRemove = promisify(this.#redisClient.srem).bind(this.#redisClient);
        this.setIsMember = promisify(this.#redisClient.sismember).bind(this.#redisClient);

        this.hashGet = promisify(this.#redisClient.hget).bind(this.#redisClient);
        this.hashSet = promisify(this.#redisClient.hset).bind(this.#redisClient);

        this.expire = promisify(this.#redisClient.expire).bind(this.#redisClient);
        this.increment = promisify(this.#redisClient.incr).bind(this.#redisClient);
        this.decrement = promisify(this.#redisClient.decr).bind(this.#redisClient);
    }

    multi(): redis.Multi & { execute: () => Promise<any[]> } {
        const multi = this.#redisClient.multi() as redis.Multi & { execute: () => Promise<any[]> };

        multi.execute = promisify(multi.exec).bind(multi);

        return multi;
    }
}
