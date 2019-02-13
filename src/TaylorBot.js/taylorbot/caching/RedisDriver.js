'use strict';

const { promisify } = require('util');
const redis = require('redis');

const redisConfig = require('../config/redis.json');

class RedisDriver {
    constructor() {
        this.redisClient = redis.createClient({
            host: redisConfig.HOST,
            port: redisConfig.PORT,
            password: redisConfig.PASSWORD
        });

        this.get = promisify(this.redisClient.get).bind(this.redisClient);
        this.setExpire = promisify(this.redisClient.setex).bind(this.redisClient);

        this.delete = promisify(this.redisClient.del).bind(this.redisClient);
        this.exists = promisify(this.redisClient.exists).bind(this.redisClient);

        this.setAdd = promisify(this.redisClient.sadd).bind(this.redisClient);
        this.setRemove = promisify(this.redisClient.srem).bind(this.redisClient);
        this.setIsMember = promisify(this.redisClient.sismember).bind(this.redisClient);
    }

    multi() {
        const multi = this.redisClient.multi();

        multi.execute = promisify(multi.exec).bind(multi);

        return multi;
    }
}

module.exports = RedisDriver;